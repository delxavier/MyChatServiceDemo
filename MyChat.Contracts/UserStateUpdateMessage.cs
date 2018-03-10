// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserStateUpdateMessage.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    This class defines the message sent my server when an user state is updated.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Contracts
{
    /// <summary>
    /// This class defines the message sent my server when an user state is updated.
    /// </summary>
    public sealed class UserStateUpdateMessage
    {
        /// <summary>
        /// Gets or sets the user ID.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="UserState"/>.
        /// </summary>
        public UserState State { get; set; }
    }
}
