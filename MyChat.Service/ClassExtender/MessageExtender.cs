// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageExtender.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    This class contains method extender for <see cref="Message"/>.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Service.ClassExtender
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MyChat.Contracts;
    using MyChat.Service.Model;

    /// <summary>
    /// This class contains method extender for <see cref="Message"/>.
    /// </summary>
    internal static class MessageExtender
    {
        /// <summary>
        /// Converts a <see cref="Message"/> to a <see cref="MessageContract"/>.
        /// </summary>
        /// <param name="message">The <see cref="Message"/> to convert.</param>
        /// <returns>The <see cref="MessageContract"/>.</returns>
        public static MessageContract ToContract(this Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(paramName: nameof(message));
            }

            if (string.IsNullOrEmpty(value: message.Content))
            {
                throw new ArgumentNullException(paramName: nameof(message), message: "message content can't be null");
            }

            return new MessageContract
            {
                Content = message.Content,
                DateTime = message.DateTime,
                OwnerId = message.OwnerId
            };
        }

        /// <summary>
        /// Converts a list of <see cref="Message"/> to a list of <see cref="MessageContract"/>.
        /// </summary>
        /// <param name="messages">The list of <see cref="Message"/> to convert.</param>
        /// <returns>The list of <see cref="MessageContract"/> converted.</returns>
        public static MessageContract[] ToContracts(this IReadOnlyCollection<Message> messages)
        {
            if (messages == null)
            {
                throw new ArgumentNullException(paramName: nameof(messages));
            }

            return messages.Where(predicate: user => user != null).Select(selector: user => user.ToContract()).ToArray();
        }

        /// <summary>
        /// Converts a <see cref="MessageContract"/> to a <see cref="Message"/>.
        /// </summary>
        /// <param name="contract">The <see cref="MessageContract"/> to convert.</param>
        /// <returns>The <see cref="Message"/>.</returns>
        public static Message FromContract(this MessageContract contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(paramName: nameof(contract));
            }

            if (string.IsNullOrEmpty(value: contract.Content))
            {
                throw new ArgumentNullException(paramName: nameof(contract), message: "message content can't be null");
            }

            return new Message(ownerId: contract.OwnerId, content: contract.Content, dateTime: contract.DateTime);
        }
    }
}
