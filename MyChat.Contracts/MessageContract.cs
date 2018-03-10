// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageContract.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    This class defines a message.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Contracts
{
    using System;

    /// <summary>
    /// This class defines a message.
    /// </summary>
    public sealed class MessageContract
    {
        /// <summary>
        /// Gets or sets the message owner ID.
        /// </summary>
        public int OwnerId { get; set; }

        /// <summary>
        /// Gets or sets the message content.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the message time.
        /// </summary>
        public DateTime DateTime { get; set; }
    }
}
