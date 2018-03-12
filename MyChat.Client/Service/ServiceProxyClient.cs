// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceProxyClient.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    This class defines the proxy for <see cref="IMyChatService"/>.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Client.Service
{
    using System;
    using System.ServiceModel;
    using MyChat.Contracts;

    /// <summary>
    /// This class defines the proxy for <see cref="IMyChatService"/>.
    /// </summary>
    internal sealed class ServiceProxyClient : ClientBase<IMyChatService>, IMyChatService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProxyClient"/> class.
        /// </summary>
        public ServiceProxyClient()
        {
            this.Open();
        }

        /// <summary>
        /// Opens a session for an user (creates a new user or connects an existing user).
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <returns>The <see cref="ConnectionResult"/>.</returns>
        public ConnectionResult OpenSession(string userName)
        {
            return this.Channel.OpenSession(userName: userName);
        }

        /// <summary>
        /// Closes a session for an user (disconnects user from chat).
        /// </summary>
        public void CloseSession()
        {
            this.Channel.CloseSession();
        }

        /// <summary>
        /// Deletes an user.
        /// </summary>
        public void LeaveChat()
        {
            this.Channel.LeaveChat();
        }

        /// <summary>
        /// Loads all connected <see cref="UserContract"/>.
        /// </summary>
        /// <returns>The list of connected <see cref="UserContract"/>.</returns>
        public UserContract[] LoadUsers()
        {
            return this.Channel.LoadUsers();
        }

        /// <summary>
        /// Loads an <see cref="UserContract"/> by its ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>The <see cref="UserContract"/>.</returns>
        public UserContract LoadUser(int userId)
        {
            return this.Channel.LoadUser(userId: userId);
        }

        /// <summary>
        /// Updates the profile of an <see cref="UserContract"/>.
        /// </summary>
        /// <param name="user">The <see cref="UserContract"/>.</param>
        public void UpdateMyProfile(UserContract user)
        {
            this.Channel.UpdateMyProfile(user: user);
        }

        /// <summary>
        /// Loads previous <see cref="MessageContract"/> before the given date.
        /// </summary>
        /// <param name="dateTime">The time limit.</param>
        /// <returns>The list of <see cref="MessageContract"/>.</returns>
        public MessageContract[] LoadPreviousMessages(DateTime dateTime)
        {
            return this.Channel.LoadPreviousMessages(dateTime: dateTime);
        }

        /// <summary>
        /// Sends a new <see cref="MessageContract"/>.
        /// </summary>
        /// <param name="message">The new <see cref="MessageContract"/>.</param>
        public void SendMessage(MessageContract message)
        {
            this.Channel.SendMessage(message: message);
        }

        /// <summary>
        /// Starts to write a message.
        /// </summary>
        public void StartWrite()
        {
            this.Channel.StartWrite();
        }

        /// <summary>
        /// Cancels to write a message.
        /// </summary>
        public void CancelWrite()
        {
            this.Channel.CancelWrite();
        }
    }
}
