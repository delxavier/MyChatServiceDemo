// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserUpdateEventArgs.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    This class defines the event args sent when a new user update is received.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Client
{
    using System;
    using MyChat.Client.Model;

    /// <summary>
    /// This class defines the event args sent when a new user update is received.
    /// </summary>
    internal sealed class UserUpdateEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserUpdateEventArgs"/> class.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="state">The <see cref="UserState"/>.</param>
        public UserUpdateEventArgs(int userId, UserState state)
        {
            this.UserId = userId;
            this.State = state;
            this.FullUpdate = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserUpdateEventArgs"/> class.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        public UserUpdateEventArgs(int userId)
        {
            this.UserId = userId;
            this.FullUpdate = true;
        }

        /// <summary>
        /// Gets the user ID.
        /// </summary>
        public int UserId { get; }

        /// <summary>
        /// Gets a value indicating whether if it's a full update or only user state update.
        /// </summary>
        public bool FullUpdate { get; }

        /// <summary>
        /// Gets the <see cref="UserState"/>.
        /// </summary>
        public UserState State { get; }
    }
}
