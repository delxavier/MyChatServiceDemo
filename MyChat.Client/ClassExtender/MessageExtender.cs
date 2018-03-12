// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageExtender.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    This class contains method extender for <see cref="Message"/>.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Client.ClassExtender
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MyChat.Client.Model;
    using MyChat.Contracts;

    /// <summary>
    /// This class contains method extender for <see cref="Message"/>.
    /// </summary>
    internal static class MessageExtender
    {
        /// <summary>
        /// Converts a list of <see cref="MessageContract"/> to a list of <see cref="Message"/>.
        /// </summary>
        /// <param name="messages">The list of <see cref="MessageContract"/> to convert.</param>
        /// <returns>The list of <see cref="Message"/> converted.</returns>
        public static IReadOnlyCollection<Message> FromContracts(this IReadOnlyCollection<MessageContract> messages)
        {
            if (messages == null)
            {
                throw new ArgumentNullException(paramName: nameof(messages));
            }

            return messages.Where(predicate: message => message != null).Select(selector: message => message.FromContract()).ToArray();
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
