// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommunicationManager.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    This is an implementation of the <see cref="ICommunicationManager"/>.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Client.Service
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net.WebSockets;
    using System.ServiceModel;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using MyChat.Client.ClassExtender;
    using MyChat.Client.Logging;
    using MyChat.Client.Model;
    using MyChat.Client.Properties;
    using MyChat.Contracts;
    using Newtonsoft.Json;

    /// <summary>
    /// This is an implementation of the <see cref="ICommunicationManager"/>.
    /// </summary>
    internal sealed class CommunicationManager : IDisposable, ICommunicationManager
    {
        /// <summary> The disposing timeout. </summary>
        private static readonly TimeSpan DisposeTimeout = TimeSpan.FromSeconds(value: 10);

        /// <summary> The lock timeout. </summary>
        private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(value: 5);
        
        /// <summary> The <see cref="ILogger"/> instance. </summary>
        private readonly ILogger logger;

        /// <summary> The reading <see cref="Task"/>. </summary>
        private readonly Task readingTask;

        /// <summary> A <see cref="ReaderWriterLockSlim"/> to manager multi-thread access. </summary>
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        /// <summary> The event to notify new connection. </summary>
        private readonly ManualResetEvent connectEvent = new ManualResetEvent(initialState: false);

        /// <summary> The event to abort communication. </summary>
        private readonly AutoResetEvent abortEvent = new AutoResetEvent(initialState: false);

        /// <summary> The cancellation token source. </summary>
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        /// <summary> The <see cref="ServiceProxyClient"/> instance. </summary>
        private ServiceProxyClient client;

        /// <summary> Indicating whether if the service is disposed. </summary>
        private volatile bool disposed;

        /// <summary> The reading <see cref="Thread"/>. </summary>
        private volatile Thread readingThread;

        /// <summary> The current user. </summary>
        private User currentUser;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationManager"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance.</param>
        public CommunicationManager(ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(paramName: nameof(logger));
            this.readingTask = Task.Factory.StartNew(
                action: this.ReadWebService,
                cancellationToken: this.cancellationTokenSource.Token,
                creationOptions: TaskCreationOptions.LongRunning,
                scheduler: TaskScheduler.Default);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="CommunicationManager"/> class. 
        /// </summary>
        ~CommunicationManager()
        {
            this.Dispose(disposing: false);
        }

        /// <summary>
        /// Occurs when a new message is received.
        /// </summary>
        public event EventHandler<MessageEventArgs> OnNewMessage
        {
            add { this.NewMessageEvent += value; }
            remove { this.NewMessageEvent -= value; }
        }

        /// <summary>
        /// Occurs when an user is updated.
        /// </summary>
        public event EventHandler<UserUpdateEventArgs> OnUserUpdate
        {
            add { this.UserUpdateEvent += value; }
            remove { this.UserUpdateEvent -= value; }
        }

        /// <summary> The event for user update. </summary>
        private event EventHandler<UserUpdateEventArgs> UserUpdateEvent;

        /// <summary> The event for new message. </summary>
        private event EventHandler<MessageEventArgs> NewMessageEvent;

        /// <summary>
        /// Gets a value indicating whether if this item is disposed.
        /// </summary>
        public bool Disposed
        {
            get { return this.disposed; }
        }

        /// <summary>
        /// Gets a value indicating whether if the connection is opened.
        /// </summary>
        public bool ConnectionOpened
        {
            get { return this.client != null && (this.client.State == CommunicationState.Opened || this.client.State == CommunicationState.Created); }
        }

        /// <summary>
        /// Gets the current user.
        /// </summary>
        public User CurrentUser
        {
            get
            {
                User user = null;
                if (this.locker.TryEnterReadLock(timeout: LockTimeout))
                {
                    user = this.currentUser;
                    this.locker.ExitReadLock();
                }

                return user;
            }
        }

        /// <summary>
        /// Gets a value indicating whether an abort request has been raised.
        /// </summary>
        private bool Aborted
        {
            get { return this.abortEvent.WaitOne(millisecondsTimeout: 0); }
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
        /// Opens connection to web service.
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <returns>The waiting <see cref="Task"/>.</returns>
        public async Task ConnectAsync(string userName)
        {
            if (userName == null)
            {
                throw new ArgumentNullException(paramName: nameof(userName));
            }

            await this.CloseAsync();

            await Task.Run(
                action: () =>
                {
                    this.ManageException(
                        action: () =>
                        {
                            this.client = new ServiceProxyClient();
                            var connectionResult = this.client.OpenSession(userName: userName);
                            var user = this.client.LoadUser(userId: connectionResult.UserId).FromContract();
                            if (this.locker.TryEnterWriteLock(timeout: LockTimeout))
                            {
                                this.currentUser = user;
                                this.locker.ExitWriteLock();
                            }
                            
                            this.cancellationTokenSource = new CancellationTokenSource();
                            this.connectEvent.Set();
                        },
                        description: "Open connection",
                        rethrowException: true);
                }).ContinueWith(continuationAction: task =>
                {
                    if (task.Exception != null)
                    {
                        throw task.Exception.InnerException;
                    }
                });
        }

        /// <summary>
        /// Closes the current connection.
        /// </summary>
        /// <returns>The waiting <see cref="Task"/>.</returns>
        public async Task CloseAsync()
        {
            await Task.Run(
                action: () =>
                {
                    this.Close();                    
                });
        }

        /// <summary>
        /// Closes the current connection.
        /// </summary>
        public void Close()
        {
            this.ManageException(
                action: () =>
                {
                    if (this.ConnectionOpened)
                    {
                        this.client.CloseSession();
                        this.client.Close();

                        this.connectEvent.Reset();
                        this.cancellationTokenSource.Cancel();
                        this.cancellationTokenSource.Dispose();
                    }
                },
                description: "Close session",
                rethrowException: false);
        }

        /// <summary>
        /// Sends a new message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>The waiting <see cref="Task"/>.</returns>
        public async Task SendMessageAsync(string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(paramName: nameof(message));
            }

            await this.CheckConnectionAsync();
            await Task.Run(
                action: () =>
                {
                    int userId = 0;
                    if (this.locker.TryEnterReadLock(timeout: LockTimeout))
                    {
                        userId = this.currentUser.UserId;
                        this.locker.ExitReadLock();
                    }

                    this.ManageException(
                        action: () => this.client.SendMessage(message: new MessageContract { Content = message, DateTime = DateTime.UtcNow, OwnerId = userId }),
                        description: "Send message");
                });
        }

        /// <summary>
        /// Loads previous <see cref="Message"/> before the given date.
        /// </summary>
        /// <param name="dateTime">The time limit.</param>
        /// <returns>The list of <see cref="Message"/>.</returns>
        public async Task<IReadOnlyCollection<Message>> LoadPreviousMessages(DateTime dateTime)
        {
            await this.CheckConnectionAsync();
            return await Task.Run(
                function: () =>
                {
                    var messages = new List<Message>();
                    this.ManageException(
                        action: () =>
                        {
                            messages.AddRange(collection: this.client.LoadPreviousMessages(dateTime: dateTime).FromContracts());
                        },
                        description: "Load messages");

                    return messages;
                });
        }

        /// <summary>
        /// Loads all connected <see cref="User"/>.
        /// </summary>
        /// <returns>The list of connected <see cref="User"/>.</returns>
        public async Task<IReadOnlyCollection<User>> LoadUsersAsync()
        {
            await this.CheckConnectionAsync();
            return await Task.Run(
                function: () =>
                {
                    var users = new List<User>();
                    this.ManageException(
                        action: () =>
                        {
                            users.AddRange(collection: this.client.LoadUsers().FromContracts());
                        },
                        description: "Load users");

                    return users;
                });
        }

        /// <summary>
        /// Loads an <see cref="User"/> by its ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>The <see cref="User"/>.</returns>
        public async Task<User> LoadUserAsync(int userId)
        {
            await this.CheckConnectionAsync();
            return await Task.Run(
                function: () =>
                {
                    User user = null;
                    this.ManageException(
                        action: () =>
                        {
                            user = this.client.LoadUser(userId: userId).FromContract();
                        },
                        description: "Load users");

                    return user;
                });
        }

        /// <summary>
        /// Updates the <see cref="Model.UserState"/> of an user.
        /// </summary>
        /// <param name="state">The <see cref="Model.UserState"/>.</param>
        /// <returns>The waiting <see cref="Task"/>.</returns>
        public async Task UpdateMyStatusAsync(Model.UserState state)
        {
            await this.CheckConnectionAsync();
            await Task.Run(
                action: () =>
                {
                    this.ManageException(
                        action: () =>
                        {
                            if (state == Model.UserState.Writing)
                            {
                                this.client.StartWrite();
                            }
                            else if (state == Model.UserState.Online)
                            {
                                this.client.CancelWrite();
                            }
                        },
                        description: "Update profile");
                });
        }

        /// <summary>
        /// Updates the profile of an <see cref="User"/>.
        /// </summary>
        /// <param name="user">The <see cref="User"/>.</param>
        /// <returns>The waiting <see cref="Task"/>.</returns>
        public async Task UpdateMyProfileAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(paramName: nameof(user));
            }

            await this.CheckConnectionAsync();
            await Task.Run(
                action: () =>
                {
                    this.ManageException(
                        action: () =>
                        {
                            var contract = user.ToContract();
                            this.client.UpdateMyProfile(user: contract);
                            if (this.locker.TryEnterWriteLock(timeout: LockTimeout))
                            {
                                this.currentUser = user;
                                this.locker.ExitWriteLock();
                            }
                        },
                        description: "Update profile",
                        rethrowException: true);
                });
        }

        /// <summary>
        /// Checks connection state.
        /// </summary>
        /// <returns>The waiting <see cref="Task"/>.</returns>
        private async Task CheckConnectionAsync()
        {
            if (!this.ConnectionOpened)
            {
                string userName = null;
                if (this.locker.TryEnterReadLock(timeout: LockTimeout))
                {
                    userName = this.currentUser?.UserName;
                    this.locker.ExitReadLock();
                }

                if (string.IsNullOrEmpty(value: userName))
                {
                    throw new NotConnectedException();
                }

                await this.ConnectAsync(userName: userName);
            }
        }

        /// <summary>
        /// Aborts the listener.
        /// </summary>
        private void Abort()
        {
            this.abortEvent.Set();
            this.cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Core task of the reading web service.
        /// </summary>
        private void ReadWebService()
        {
            try
            {
                this.readingThread = Thread.CurrentThread; // Permit old way thread aborting in disposition code since Task interface does not allow aborting.
                this.logger.Write(logMessage: "Starts reading task ...");
                this.WaitIncomingData();
            }
            catch (ThreadAbortException exception)
            {
                this.logger.WriteException(exception, SeverityLevel.Warning, "Reading task has been aborted");
            }
            catch (Exception exception)
            {
                this.logger.WriteException(exception, SeverityLevel.Failure, "Unexpected error while running reading task, no more processing is done");
                throw;
            }
            finally
            {
                this.logger.Write(SeverityLevel.Debug, "Leaving reading task");
            }
        }

        /// <summary>
        /// Waits incoming data from web service.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Handled by log.")]
        private void WaitIncomingData()
        {
            do
            {
                TimeSpan timeout = TimeSpan.Zero;
                try
                {
                    if (this.connectEvent.WaitOne())
                    {
                        using (var webSocket = new ClientWebSocket())
                        {
                            string userName = null;
                            if (this.locker.TryEnterReadLock(timeout: LockTimeout))
                            {
                                userName = this.currentUser?.UserName;
                                this.locker.ExitReadLock();
                            }

                            webSocket.ConnectAsync(uri: new Uri(uriString: Settings.Default.WebSocketUri), cancellationToken: this.cancellationTokenSource.Token).Wait();
                            webSocket.SendAsync(
                                buffer: new ArraySegment<byte>(array: Encoding.UTF8.GetBytes(s: userName ?? string.Empty)),
                                messageType: WebSocketMessageType.Text,
                                endOfMessage: true,
                                cancellationToken: this.cancellationTokenSource.Token);

                            do
                            {
                                try
                                {
                                    if (this.Aborted)
                                    {
                                        this.logger.Write(logMessage: "Reading task is aborted ...");
                                        return;
                                    }

                                    var buffer = new List<byte>();
                                    this.TryReceiveData(webSocket: webSocket, readBytes: buffer);
                                    this.ParseReceivedData(buffer: buffer);
                                }
                                catch (AggregateException exception)
                                {
                                    WebSocketException socketException = exception.InnerExceptions.OfType<WebSocketException>().FirstOrDefault();
                                    if (socketException != null)
                                    {
                                        this.logger.Write(
                                            severity: SeverityLevel.Error,
                                            logMessage: "There are errors on websocket. Close it and recreate a new websocket");

                                        // There are an error. Adds seconds to wait.
                                        timeout = TimeSpan.FromSeconds(value: 30d);
                                        break;
                                    }

                                    if (!this.Aborted)
                                    {
                                        this.logger.WriteException(
                                            exception: exception,
                                            severity: SeverityLevel.Error,
                                            logMessage: "Unexpected error while running reading task");
                                    }
                                }
                                catch (Exception exception)
                                {
                                    if (!this.Aborted)
                                    {
                                        this.logger.WriteException(
                                            exception: exception,
                                            severity: SeverityLevel.Error,
                                            logMessage: "Unexpected error while running reading task");
                                    }
                                }
                            }
                            while (!this.Aborted && !this.cancellationTokenSource.IsCancellationRequested);
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (!this.Aborted)
                    {
                        this.logger.WriteException(
                            exception: exception,
                            severity: SeverityLevel.Error,
                            logMessage: "Unexpected error while openning socket. Retry to open in few time");
                    }

                    // There are an error. Adds seconds to wait.
                    timeout = TimeSpan.FromSeconds(value: 30d);
                }

                this.abortEvent.WaitOne(timeout: timeout);
            }
            while (!this.Aborted);
        }

        /// <summary>
        /// Tries to read data from the <see cref="WebSocket"/>.
        /// </summary>
        /// <param name="webSocket">The <see cref="WebSocket"/> instance.</param>
        /// <param name="readBytes">The read bytes from the <see cref="WebSocket"/>.</param>
        private void TryReceiveData(WebSocket webSocket, List<byte> readBytes)
        {
            if (webSocket == null)
            {
                throw new ArgumentNullException(paramName: nameof(webSocket));
            }

            if (readBytes == null)
            {
                throw new ArgumentNullException(paramName: nameof(readBytes));
            }

            var buffer = new ArraySegment<byte>(array: new byte[1024]);
            Task<WebSocketReceiveResult> receivedTask = webSocket.ReceiveAsync(buffer: buffer, cancellationToken: this.cancellationTokenSource.Token);
            WebSocketReceiveResult receivedResult = receivedTask.Result;
            readBytes.AddRange(collection: buffer.Take(count: receivedResult.Count).ToArray());
            if (!receivedResult.EndOfMessage && !this.cancellationTokenSource.IsCancellationRequested)
            {
                // The message is not complete.
                // Waits the end of message;
                this.TryReceiveData(webSocket: webSocket, readBytes: readBytes);
            }
        }

        /// <summary>
        /// Parses received data.
        /// </summary>
        /// <param name="buffer">The received data.</param>
        private void ParseReceivedData(IEnumerable<byte> buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(paramName: nameof(buffer));
            }

            string receivedData = Encoding.UTF8.GetString(bytes: buffer.ToArray());
            if (receivedData.Contains(value: "Content") || receivedData.Contains(value: "content"))
            {
                var message = JsonConvert.DeserializeObject<MessageContract>(value: receivedData);
                this.NewMessageEvent?.Invoke(sender: this, e: new MessageEventArgs(message: message.FromContract()));
            }
            else if (receivedData.Contains(value: "State") || receivedData.Contains(value: "state"))
            {
                var message = JsonConvert.DeserializeObject<UserStateUpdateMessage>(value: receivedData);
                this.UserUpdateEvent?.Invoke(sender: this, e: new UserUpdateEventArgs(userId: message.UserId, state: (Model.UserState)(int)message.State));
            }
            else if (receivedData.Contains(value: "UserId") || receivedData.Contains(value: "userid"))
            {
                var message = JsonConvert.DeserializeObject<UserProfileUpdateMessage>(value: receivedData);
                this.UserUpdateEvent?.Invoke(sender: this, e: new UserUpdateEventArgs(userId: message.UserId));
            }
        }

        /// <summary>
        /// Manages exception during a call.
        /// </summary>
        /// <param name="action">The <see cref="Action"/> to run.</param>
        /// <param name="description">The method name.</param>
        /// <param name="rethrowException">True to rethrow exception otherwise false.</param>
        private void ManageException(Action action, string description, bool rethrowException = false)
        {
            try
            {
                this.logger.Format(severity: SeverityLevel.Info, logMessageFormat: "Calling {0}...", args: description);
                action();
                this.logger.Format(severity: SeverityLevel.Info, logMessageFormat: "Call to {0} completed", args: description);
            }
            catch (Exception exception)
            {
                this.logger.FormatException(
                    exception: exception,
                    severity: SeverityLevel.Failure,
                    logMessageFormat: "An error occurs during call {0}",
                    args: description);

                if (rethrowException)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Handled by log and don't want exception in disposition")]
        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.Abort();
                    try
                    {
                        if (this.readingTask.Wait(timeout: DisposeTimeout))
                        {
                            this.readingTask.Dispose();
                        }
                        else
                        {
                            this.logger.Format(
                                severity: SeverityLevel.Error,
                                logMessageFormat: "Reading task did not respond to cancelation request within {0}, killing it",
                                args: DisposeTimeout);
                            if (this.readingThread != null)
                            {
                                // May result in corrupted state domain.
                                this.readingThread.Abort();
                            }
                            else
                            {
                                this.logger.Write(
                                    severity: SeverityLevel.Warning,
                                    logMessage: "Could not kill the reading task");
                            }
                        }

                        this.CloseAsync().Wait();
                        this.abortEvent.Dispose();
                        this.connectEvent.Dispose();
                        this.locker.Dispose();
                        this.cancellationTokenSource.Dispose();
                    }
                    catch (AggregateException exceptions)
                    {
                        exceptions.Handle(exception =>
                        {
                            this.logger.WriteException(
                                exception: exception,
                                severity: SeverityLevel.Error,
                                logMessage: "Unhandled exception in reading task");
                            return true;
                        });
                    }
                    catch (Exception exception)
                    {
                        this.logger.WriteException(
                            exception: exception,
                            severity: SeverityLevel.Error,
                            logMessage: "Unexpected error while disposing reading task");
                    }
                }

                this.disposed = true;
            }
        }
    }
}
