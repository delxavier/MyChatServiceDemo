// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageEventArgs.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    This class defines the event args sent when a new message is received.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Client
{
    using System;
    using MyChat.Client.Model;

    /// <summary>
    /// This class defines the event args sent when a new message is received.
    /// </summary>
    internal sealed class MessageEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageEventArgs"/> class.
        /// </summary>
        /// <param name="message">The <see cref="Message"/>.</param>
        public MessageEventArgs(Message message)
        {
            this.Message = message ?? throw new ArgumentNullException(paramName: nameof(message));
        }

        /// <summary>
        /// Gets the <see cref="Message"/>.
        /// </summary>
        public Message Message { get; }
    }
}
