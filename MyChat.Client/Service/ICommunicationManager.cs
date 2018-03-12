// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICommunicationManager.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    This interface defines the communication manager.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Client.Service
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using MyChat.Client.Model;

    /// <summary>
    /// This interface defines the communication manager.
    /// </summary>
    internal interface ICommunicationManager
    {
        /// <summary>
        /// Occurs when a new message is received.
        /// </summary>
        event EventHandler<MessageEventArgs> OnNewMessage;

        /// <summary>
        /// Occurs when an user is updated.
        /// </summary>
        event EventHandler<UserUpdateEventArgs> OnUserUpdate;

        /// <summary>
        /// Gets a value indicating whether if the connection is opened.
        /// </summary>
        bool ConnectionOpened { get; }

        /// <summary>
        /// Gets the current user.
        /// </summary>
        User CurrentUser { get; }

        /// <summary>
        /// Opens connection to web service.
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <returns>The waiting <see cref="Task"/>.</returns>
        Task ConnectAsync(string userName);

        /// <summary>
        /// Closes the current connection.
        /// </summary>
        /// <returns>The waiting <see cref="Task"/>.</returns>
        Task CloseAsync();

        /// <summary>
        /// Closes the current connection.
        /// </summary>
        void Close();

        /// <summary>
        /// Sends a new message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>The waiting <see cref="Task"/>.</returns>
        Task SendMessageAsync(string message);

        /// <summary>
        /// Loads previous <see cref="Message"/> before the given date.
        /// </summary>
        /// <param name="dateTime">The time limit.</param>
        /// <returns>The list of <see cref="Message"/>.</returns>
        Task<IReadOnlyCollection<Message>> LoadPreviousMessages(DateTime dateTime);

        /// <summary>
        /// Loads all connected <see cref="User"/>.
        /// </summary>
        /// <returns>The list of connected <see cref="User"/>.</returns>
        Task<IReadOnlyCollection<User>> LoadUsersAsync();

        /// <summary>
        /// Loads an <see cref="User"/> by its ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>The <see cref="User"/>.</returns>
        Task<User> LoadUserAsync(int userId);

        /// <summary>
        /// Updates the <see cref="Model.UserState"/> of an user.
        /// </summary>
        /// <param name="state">The <see cref="Model.UserState"/>.</param>
        /// <returns>The waiting <see cref="Task"/>.</returns>
        Task UpdateMyStatusAsync(UserState state);

        /// <summary>
        /// Updates the profile of an <see cref="User"/>.
        /// </summary>
        /// <param name="user">The <see cref="User"/>.</param>
        /// <returns>The waiting <see cref="Task"/>.</returns>
        Task UpdateMyProfileAsync(User user);
    }
}
