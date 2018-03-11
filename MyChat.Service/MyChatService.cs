// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MyChatService.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    This class im describes the chat service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Service
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.ServiceModel;
    using System.Threading;
    using MyChat.Contracts;
    using MyChat.Service.ClassExtender;
    using MyChat.Service.Logging;
    using MyChat.Service.Model;

    /// <summary>
    /// This class implements the <see cref="IMyChatService"/>.
    /// </summary>
    [ServiceBehavior(
                     ConcurrencyMode = ConcurrencyMode.Multiple,
                     InstanceContextMode = InstanceContextMode.PerSession)]
    internal sealed class MyChatService : IMyChatService, IDisposable
    {
        /// <summary> The count of instances running. </summary>
        private static int runningInstances;

        /// <summary> An unique session ID. </summary>
        private readonly Guid sessionId = Guid.NewGuid();

        /// <summary> The <see cref="ILogger"/> instance. </summary>
        private readonly ILogger logger;

        /// <summary> The <see cref="IDataStore"/> instance. </summary>
        private readonly IDataStore store;

        /// <summary> The <see cref="INotificationManager"/> instance. </summary>
        private readonly INotificationManager notificationManager;

        /// <summary> The connected user ID. </summary>
        private int currentUserId;

        /// <summary> A flag to know if session is closed by user. </summary>
        private bool sessionCloseByUser = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyChatService"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance.</param>
        /// <param name="store">The <see cref="IDataStore"/> instance.</param>
        /// <param name="notificationManager">The <see cref="INotificationManager"/> instance.</param>
        public MyChatService(ILogger logger, IDataStore store, INotificationManager notificationManager)
        {
            this.logger = logger ?? throw new ArgumentNullException(paramName: nameof(logger));
            this.store = store ?? throw new ArgumentNullException(paramName: nameof(store));
            this.notificationManager = notificationManager ?? throw new ArgumentNullException(paramName: nameof(notificationManager));

            this.logger.Format(                
                SeverityLevel.Debug,
                this.sessionId.GetHashCode(),
                "New MyChatService instance n°{0} is created ({1})",
                Interlocked.Increment(ref runningInstances),
                this.sessionId);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="MyChatService"/> class. 
        /// </summary>
        ~MyChatService()
        {
            this.Dispose(disposing: false);
        }

        /// <summary>
        /// Gets a value indicating whether if this item is disposed.
        /// </summary>
        public bool Disposed { get; private set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(obj: this);
        }

        /// <summary>
        /// Opens a session for an user (creates a new user or connects an existing user).
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <returns>The <see cref="ConnectionResult"/>.</returns>
        public ConnectionResult OpenSession(string userName)
        {
            return this.ManageException(
                action: () =>
                {
                    this.currentUserId = this.store.AddOrUpdateUser(user: new User { UserName = userName });
                    return new ConnectionResult { UserId = this.currentUserId };
                },
                description: "OpenSession");
        }

        /// <summary>
        /// Closes a session for an user (disconnects user from chat).
        /// </summary>
        public void CloseSession()
        {
            this.ManageException(
                action: () =>
                {
                    this.store.UpdateUserState(userId: this.currentUserId, state: Model.UserState.Offline);
                    this.notificationManager.NotifyUserStateChange(userId: this.currentUserId, state: Model.UserState.Offline);
                    this.sessionCloseByUser = true;
                },
                description: "CloseSession");
        }

        /// <summary>
        /// Deletes an user.
        /// </summary>
        public void LeaveChat()
        {
            this.ManageException(
                action: () =>
                {
                    this.store.DeleteUser(userId: this.currentUserId);
                    this.notificationManager.NotifyUserStateChange(userId: this.currentUserId, state: Model.UserState.Deleted);
                    this.sessionCloseByUser = true;
                },
                description: "LeaveChat");
        }

        /// <summary>
        /// Loads all connected <see cref="UserContract"/>.
        /// </summary>
        /// <returns>The list of connected <see cref="UserContract"/>.</returns>
        public UserContract[] LoadUsers()
        {
            return this.ManageException(
                action: () => this.store.LoadAllUsers().ToContracts(),
                description: "LoadUsers");
        }

        /// <summary>
        /// Loads an <see cref="UserContract"/> by its ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>The <see cref="UserContract"/>.</returns>
        public UserContract LoadUser(int userId)
        {
            return this.ManageException(
                action: () => this.store.LoadUser(userId: userId).ToContract(),
                description: "LoadUser");
        }

        /// <summary>
        /// Updates the profile of an <see cref="UserContract"/>.
        /// </summary>
        /// <param name="user">The <see cref="UserContract"/>.</param>
        public void UpdateMyProfile(UserContract user)
        {
            this.ManageException(
                action: () =>
                {
                    if (user == null)
                    {
                        throw new InvalidOperationException(message: "user can't be null");
                    }

                    if (user.UserId <= 0)
                    {
                        throw new InvalidOperationException(message: "unknown user");
                    }

                    if (user.UserId != this.currentUserId)
                    {
                        throw new InvalidOperationException(message: "not allowed to update profile");
                    }

                    this.store.AddOrUpdateUser(user: user.FromContract());
                    this.notificationManager.NotifyUserChange(userId: user.UserId);
                },
                description: "UpdateMyProfile");
        }

        /// <summary>
        /// Loads previous <see cref="MessageContract"/> before the given date.
        /// </summary>
        /// <param name="dateTime">The time limit.</param>
        /// <returns>The list of <see cref="MessageContract"/>.</returns>
        public MessageContract[] LoadPreviousMessages(DateTime dateTime)
        {
            return this.ManageException(
                action: () => this.store.LoadPreviousMessages(dateTime: dateTime).ToContracts(),
                description: "LoadPreviousMessages");
        }

        /// <summary>
        /// Sends a new <see cref="MessageContract"/>.
        /// </summary>
        /// <param name="message">The new <see cref="MessageContract"/>.</param>
        public void SendMessage(MessageContract message)
        {
            this.ManageException(
                action: () =>
                {
                    var newMessage = message.FromContract();
                    this.store.AddMessage(message: newMessage);
                    this.notificationManager.NotifyNewMessage(message: newMessage);
                },
                description: "SendMessage");
        }

        /// <summary>
        /// Starts to write a message.
        /// </summary>
        public void StartWrite()
        {
            this.ManageException(
                action: () =>
                {
                    this.store.UpdateUserState(userId: this.currentUserId, state: Model.UserState.Writing);
                    this.notificationManager.NotifyUserStateChange(userId: this.currentUserId, state: Model.UserState.Writing);
                },
                description: "StartWrite");
        }

        /// <summary>
        /// Cancels to write a message.
        /// </summary>
        public void CancelWrite()
        {
            this.ManageException(
                action: () =>
                {
                    this.store.UpdateUserState(userId: this.currentUserId, state: Model.UserState.Online);
                    this.notificationManager.NotifyUserStateChange(userId: this.currentUserId, state: Model.UserState.Online);
                },
                description: "CancelWrite");
        }

        /// <summary>
        /// Manages exception during a method call.
        /// </summary>
        /// <param name="action">The method to execute.</param>
        /// <param name="description">The method description.</param>
        private void ManageException(Action action, string description)
        {
            this.ManageException(
                action: () =>
                {
                    action();
                    return string.Empty;
                },
                description: description);
        }

        /// <summary>
        /// Manages exception during a method call.
        /// </summary>
        /// <typeparam name="TResult">The result object.</typeparam>
        /// <param name="action">The method to execute.</param>
        /// <param name="description">The method description.</param>
        /// <returns>The call result.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Handled by log and return a FaultException<GpsBoxWebListenerError>.")]
        private TResult ManageException<TResult>(Func<TResult> action, string description) where TResult : class
        {
            if (action == null)
            {
                throw new ArgumentNullException(paramName: nameof(action));
            }

            if (description == null)
            {
                throw new ArgumentNullException(paramName: nameof(description));
            }

            long trackInfo = this.sessionId.GetHashCode();
            this.logger.Format(
                severity: SeverityLevel.Info,
                trackInfo: trackInfo,
                logMessageFormat: "Calling client service for method {0} ...",
                args: description);

            try
            {
                TResult result = action();

                this.logger.Format(                    
                    severity: SeverityLevel.Info,
                    trackInfo: trackInfo,
                    logMessageFormat: "Call client service for method {0} complete ...",
                    args: description);

                return result;
            }
            catch (FaultException<ChatServiceError> exception)
            {
                this.logger.WriteException(
                    exception: exception,
                    trackInfo: trackInfo,
                    severity: SeverityLevel.Error,
                    logMessage: "Exception thrown to client from service method");
                throw;
            }
            catch (FaultException exception)
            {
                this.logger.WriteException(
                    exception: exception,
                    trackInfo: trackInfo,
                    severity: SeverityLevel.Error,
                    logMessage: "Exception without details thrown to client from service method");

                throw FaultExceptionHelper.From(exception: exception);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                this.logger.WriteException(
                   exception: exception,
                   trackInfo: this.sessionId.GetHashCode(),
                   severity: SeverityLevel.Error,
                   logMessage: "Unknown ID Exception exception thrown to client from method, converting it to fault");

                throw FaultExceptionHelper.From(error: ErrorType.InvalidArgument, exception: exception);
            }
            catch (InvalidOperationException exception)
            {
                this.logger.WriteException(
                    exception: exception,
                    trackInfo: this.sessionId.GetHashCode(),
                    severity: SeverityLevel.Failure,
                    logMessage: "Exception thrown to client from service method, converting it to fault");

                throw FaultExceptionHelper.From(exception: exception);
            }
            catch (Exception exception)
            {
                this.logger.WriteException(
                    exception: exception,
                    trackInfo: this.sessionId.GetHashCode(),
                    severity: SeverityLevel.Failure,
                    logMessage: "Unhandled exception catched from service method, converting it to fault");

                throw FaultExceptionHelper.From(exception: exception);
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">True if managed resources should be disposed; otherwise, false.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "OK here. Don't want exception in disposition code")]
        private void Dispose(bool disposing)
        {
            if (!this.Disposed)
            {
                if (disposing)
                {
                    this.logger.Format(                        
                        SeverityLevel.Debug,
                        this.sessionId.GetHashCode(),
                        "Service instance is disposed for session ({0}), {1} instances remaining",
                        this.sessionId,
                        Interlocked.Decrement(ref runningInstances));
                }

                if (!this.sessionCloseByUser)
                {
                    try
                    {
                        this.store.UpdateUserState(userId: this.currentUserId, state: Model.UserState.Offline);
                        this.notificationManager.NotifyUserStateChange(userId: this.currentUserId, state: Model.UserState.Offline);
                    }
                    catch (Exception exception)
                    {
                        this.logger.FormatException(
                            exception: exception,
                            severity: SeverityLevel.Error,
                            logMessageFormat: "An error occurs during user {0} session closed",
                            args: this.currentUserId);
                    }
                }

                this.Disposed = true;
            }
        }
    }
}
