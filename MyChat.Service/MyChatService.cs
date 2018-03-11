﻿// --------------------------------------------------------------------------------------------------------------------
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
    internal sealed class MyChatService : IMyChatService
    {
        /// <summary> The count of instances running. </summary>
        private static int runningInstances;

        /// <summary> An unique session ID. </summary>
        private readonly Guid sessionId = Guid.NewGuid();

        /// <summary> The <see cref="ILogger"/> instance. </summary>
        private readonly ILogger logger;

        /// <summary> The <see cref="IDataStore"/> instance. </summary>
        private readonly IDataStore store;

        /// <summary> The connected user ID. </summary>
        private int userId;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyChatService"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance.</param>
        /// <param name="store">The <see cref="IDataStore"/> instance.</param>
        public MyChatService(ILogger logger, IDataStore store)
        {
            this.logger = logger ?? throw new ArgumentNullException(paramName: nameof(logger));
            this.store = store ?? throw new ArgumentNullException(paramName: nameof(store));
            
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

        }

        /// <summary>
        /// Closes a session for an user (disconnects user from chat).
        /// </summary>
        public void CloseSession()
        {
            this.ManageException(
                action: () => this.store.UpdateUserState(userId: this.userId, state: Model.UserState.Offline),
                description: "CloseSession");
        }

        /// <summary>
        /// Deletes an user.
        /// </summary>
        public void LeaveChat()
        {
            this.ManageException(
                action: () => this.store.DeleteUser(userId: this.userId),
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
                action: () => this.store.AddOrUpdateUser(user: user.FromContract()),
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
                action: () => this.store.AddMessage(message: message.FromContract()),
                description: "SendMessage");
        }

        /// <summary>
        /// Starts to write a message.
        /// </summary>
        public void StartWrite()
        {
            this.ManageException(
                action: () => this.store.UpdateUserState(userId: this.userId, state: Model.UserState.Writing),
                description: "StartWrite");
        }

        /// <summary>
        /// Cancels to write a message.
        /// </summary>
        public void CancelWrite()
        {
            this.ManageException(
                action: () => this.store.UpdateUserState(userId: this.userId, state: Model.UserState.Online),
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

                this.Disposed = true;
            }
        }
    }
}
