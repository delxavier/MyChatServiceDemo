// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IConnectionManager.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    This interface defines the client connection manager.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Service.ConnectionManager
{
    using MyChat.Service.Model;

    /// <summary>
    /// This interface defines the client connection manager.
    /// </summary>
    internal interface IConnectionManager
    {
        /// <summary>
        /// Notifies all connected clients that an user state has changed.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="state">The new <see cref="UserState"/>.</param>
        void NotifyUserStateChange(int userId, UserState state);

        /// <summary>
        /// Notifies a new incoming <see cref="Message"/>.
        /// </summary>
        /// <param name="message">The <see cref="Message"/>.</param>
        void NotifyNewMessage(Message message);
    }
}
