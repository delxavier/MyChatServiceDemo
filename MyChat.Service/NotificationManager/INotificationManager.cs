// --------------------------------------------------------------------------------------------------------------------
// <copyright file="INotificationManager.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    This interface defines the client notification manager.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Service
{
    using MyChat.Service.Model;

    /// <summary>
    /// This interface defines the client notification manager.
    /// </summary>
    internal interface INotificationManager
    {
        /// <summary>
        /// Notifies all connected clients that an user state has changed.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="state">The new <see cref="UserState"/>.</param>
        void NotifyUserStateChange(int userId, UserState state);

        /// <summary>
        /// Notifies all connected clients that an user has changed.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        void NotifyUserChange(int userId);

        /// <summary>
        /// Notifies a new incoming <see cref="Message"/>.
        /// </summary>
        /// <param name="message">The <see cref="Message"/>.</param>
        void NotifyNewMessage(Message message);
    }
}
