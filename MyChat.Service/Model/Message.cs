// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Message.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    This class defines a message.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Service.Model
{
    using System;

    /// <summary>
    /// This class defines a message.
    /// </summary>
    internal sealed class Message
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="ownerId">The message owner ID.</param>
        /// <param name="content">The message content.</param>
        /// <param name="dateTime">The message time.</param>
        public Message(int ownerId, string content, DateTime dateTime)
        {
            this.OwnerId = ownerId;
            this.Content = content ?? throw new ArgumentNullException(paramName: nameof(content));
        }

        /// <summary>
        /// Gets the message owner ID.
        /// </summary>
        public int OwnerId { get; }

        /// <summary>
        /// Gets the message content.
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// Gets the message time.
        /// </summary>
        public DateTime DateTime { get; }
    }
}
