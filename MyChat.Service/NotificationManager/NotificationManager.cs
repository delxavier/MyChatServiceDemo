// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotificationManager.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    This is an implementation of the <see cref="INotificationManager"/>.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Service
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Fleck;
    using MyChat.Service.Logging;
    using MyChat.Service.Model;
    using Newtonsoft.Json;

    /// <summary>
    /// This is an implementation of the <see cref="INotificationManager"/>.
    /// </summary>
    internal sealed class NotificationManager : INotificationManager, IDisposable
    {
        /// <summary> The <see cref="ILogger"/> instance. </summary>
        private readonly ILogger logger;

        /// <summary> The <see cref="WebSocketServer"/> instance. </summary>
        private readonly WebSocketServer socketServer;

        /// <summary> The list of connected clients. </summary>
        private readonly ConcurrentDictionary<IWebSocketConnection, Tuple<string, int>> clients = new ConcurrentDictionary<IWebSocketConnection, Tuple<string, int>>();

        /// <summary> The <see cref="IDataStore"/> instance. </summary>
        private readonly IDataStore dataStore;

        /// <summary> The disposition state. </summary>
        private volatile bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationManager"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance.</param>
        /// <param name="dataStore">The <see cref="IDataStore"/> instance.</param>
        /// <param name="port">The listening port.</param>
        /// <param name="uri">The listening uri.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "OK here")]
        public NotificationManager(ILogger logger, IDataStore dataStore, int port, string uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(paramName: nameof(uri));
            }

            this.dataStore = dataStore ?? throw new ArgumentNullException(paramName: nameof(dataStore));
            this.logger = logger ?? throw new ArgumentNullException(paramName: nameof(logger));
            this.logger.Format(
                SeverityLevel.Info,
                "Initializing the websocket server on uri {0} ({1})",
                uri.ToString(),
                port);

            this.socketServer = new WebSocketServer(port: port, location: uri)
            {
                RestartAfterListenError = true
            };

            this.socketServer.Start(
                config: connection =>
                {
                    connection.OnOpen = () => this.OnNewConnection(connection: connection);
                    connection.OnMessage = (message) => this.OnNewMessage(connection: connection, message: message);
                    connection.OnClose = () => this.OnCloseConnection(connection: connection);
                    connection.OnError = (exception) => this.OnError(connection: connection, exception: exception);
                });
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="NotificationManager"/> class. 
        /// </summary>
        ~NotificationManager()
        {
            this.Dispose(disposing: false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(obj: this);
        }

        /// <summary>
        /// Notifies all connected clients that an user state has changed.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="state">The new <see cref="UserState"/>.</param>
        public void NotifyUserStateChange(int userId, UserState state)
        {
            var contract = new Contracts.UserStateUpdateMessage { UserId = userId, State = (Contracts.UserState)(int)state };
            foreach (var client in this.clients)
            {
                this.SendMessage(connection: client.Key, message: contract);
            }

            if (state == UserState.Offline || state == UserState.Deleted)
            {
                var clientsToDelete = this.clients.Where(predicate: item => item.Value.Item2 == userId);
                foreach (var client in clientsToDelete)
                {
                    this.clients.TryRemove(key: client.Key, value: out var delClient);
                }
            }
        }

        /// <summary>
        /// Notifies all connected clients that an user has changed.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        public void NotifyUserChange(int userId)
        {
            var contract = new Contracts.UserProfileUpdateMessage { UserId = userId };
            foreach (var client in this.clients)
            {
                this.SendMessage(connection: client.Key, message: contract);
            }
        }

        /// <summary>
        /// Notifies a new incoming <see cref="Message"/>.
        /// </summary>
        /// <param name="message">The <see cref="Message"/>.</param>
        public void NotifyNewMessage(Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(paramName: nameof(message));
            }

            var contract = new Contracts.MessageContract { OwnerId = message.OwnerId, Content = message.Content, DateTime = message.DateTime };
            foreach (var client in this.clients)
            {
                this.SendMessage(connection: client.Key, message: contract);
            }
        }

        /// <summary>
        /// Sends a message on client socket.
        /// </summary>
        /// <param name="connection">The <see cref="IWebSocketConnection"/>.</param>
        /// <param name="message">The message to send.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Handled in logs")]
        private void SendMessage(IWebSocketConnection connection, object message)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(paramName: nameof(connection));
            }

            if (message == null)
            {
                throw new ArgumentNullException(paramName: nameof(message));
            }

            try
            {
                connection.Send(message: JsonConvert.SerializeObject(value: message));
            }
            catch (Exception exception)
            {
                this.logger.FormatException(
                    exception: exception,
                    severity: SeverityLevel.Failure,
                    logMessageFormat: "Can't send message to client {0}",
                    args: connection.ConnectionInfo.ClientIpAddress);
            }
        }

        /// <summary>
        /// Receives a new message from a client socket.
        /// </summary>
        /// <param name="connection">The <see cref="IWebSocketConnection"/>.</param>
        /// <param name="message">The incoming message.</param>
        private void OnNewMessage(IWebSocketConnection connection, string message)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(paramName: nameof(connection));
            }

            if (string.IsNullOrEmpty(value: message))
            {
                throw new ArgumentNullException(paramName: nameof(message));
            }

            if (!this.clients.ContainsKey(key: connection))
            {
                if (!this.clients.Values.Any(predicate: item => string.Equals(a: item.Item1, b: message, comparisonType: StringComparison.OrdinalIgnoreCase)))
                {
                    if (!this.dataStore.UserExist(userName: message))
                    {
                        var answer = new Contracts.MessageContract
                        {
                            Content = "User not found",
                            DateTime = DateTime.UtcNow
                        };

                        this.SendMessage(connection: connection, message: answer);
                        connection.Close();
                        return;
                    }

                    var user = this.dataStore.LoadUser(userName: message);
                    var client = new Tuple<string, int>(item1: message, item2: user.UserId);
                    this.clients.AddOrUpdate(
                        key: connection,
                        addValue: client,
                        updateValueFactory: (key, oldValue) => client);

                    this.logger.Format(
                        SeverityLevel.Info,
                        "Client connected on address {0} has name {1}.",
                         connection.ConnectionInfo.ClientIpAddress,
                         message);

                    this.NotifyUserStateChange(userId: user.UserId, state: UserState.Online);
                }
            }
        }

        /// <summary>
        /// Opens a new client socket.
        /// </summary>
        /// <param name="connection">The new <see cref="IWebSocketConnection"/>.</param>
        private void OnNewConnection(IWebSocketConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(paramName: nameof(connection));
            }

            this.logger.Format(
                severity: SeverityLevel.Info, 
                logMessageFormat: "New client is connected on address {0}.", 
                args: connection.ConnectionInfo.ClientIpAddress);
        }

        /// <summary>
        /// Closes a client socket.
        /// </summary>
        /// <param name="connection">The closed <see cref="IWebSocketConnection"/>.</param>
        private void OnCloseConnection(IWebSocketConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(paramName: nameof(connection));
            }

            Tuple<string, int> user;
            if (this.clients.TryRemove(key: connection, value: out user))
            {
                this.logger.Format(
                    severity: SeverityLevel.Info,
                    logMessageFormat: "Closes the connection for user {0}.",
                    args: user.Item2);

                this.dataStore.UpdateUserState(userId: user.Item2, state: UserState.Offline);
                this.NotifyUserStateChange(userId: user.Item2, state: UserState.Offline);
            }
        }

        /// <summary>
        /// Manages an error occured on client.
        /// </summary>
        /// <param name="connection">The faulted <see cref="IWebSocketConnection"/>.</param>
        /// <param name="exception">The <see cref="Exception"/>.</param>
        private void OnError(IWebSocketConnection connection, Exception exception)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(paramName: nameof(connection));
            }

            Tuple<string, int> user;
            if (!this.clients.TryGetValue(key: connection, value: out user))
            {
                this.logger.WriteException(
                    exception: exception,
                    severity: SeverityLevel.Error,
                    logMessage: "One error occurs on unknown connection");
                return;
            }

            this.logger.FormatException(
                exception: exception,
                severity: SeverityLevel.Error,
                logMessageFormat: "One error occurs on socket {0}",
                args: user.Item2);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">True if managed resources should be disposed; otherwise, false.</param>
        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.logger.Write(logMessage: "Disposing the notification manager...");
                    this.socketServer.Dispose();
                }

                this.disposed = true;
            }
        }
    }
}
