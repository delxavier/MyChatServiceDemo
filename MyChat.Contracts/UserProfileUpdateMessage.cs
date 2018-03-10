// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserProfileUpdateMessage.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    This class defines the message sent my server when an user is updated.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Contracts
{
    /// <summary>
    /// This class defines the message sent my server when an user is updated.
    /// </summary>
    public sealed class UserProfileUpdateMessage
    {
        /// <summary>
        /// Gets or sets the user ID.
        /// </summary>
        public int UserId { get; set; }
    }
}
