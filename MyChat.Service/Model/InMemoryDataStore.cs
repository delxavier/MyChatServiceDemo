// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InMemoryDataStore.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    This is an implementation of the <see cref="IDataStore"/>.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Service.Model
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// This is an implementation of the <see cref="IDataStore"/>.
    /// </summary>
    internal sealed class InMemoryDataStore : IDataStore
    {
        /// <summary> The max number of messages in queue. </summary>
        private const int MaxMessages = 1000;

        /// <summary> The max number of messages returned in one read operation. </summary>
        private const int MaxMessagesRead = 20;

        /// <summary> The list of <see cref="User"/>. </summary>
        private readonly ConcurrentDictionary<int, User> users = new ConcurrentDictionary<int, User>();

        /// <summary> The list of <see cref="Message"/>. </summary>
        private readonly ConcurrentQueue<Message> messages = new ConcurrentQueue<Message>();

        /// <summary>
        /// Adds or updates an user into the datastore.
        /// </summary>
        /// <param name="user">The <see cref="User"/>.</param>
        public void AddOrUpdateUser(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(paramName: nameof(user));
            }

            if (string.IsNullOrEmpty(value: user.UserName))
            {
                throw new ArgumentNullException(paramName: nameof(user), message: "user name can't be null");
            }

            this.users.AddOrUpdate(key: user.UserId, addValue: user, updateValueFactory: (key, oldValue) => user);
        }

        /// <summary>
        /// Updates an user state.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="state">The new <see cref="UserState"/>.</param>
        public void UpdateUserState(int userId, UserState state)
        {
            User user;
            if (!this.users.TryGetValue(key: userId, value: out user))
            {
                throw new ArgumentOutOfRangeException(paramName: nameof(userId), message: "User not found");
            }

            user.State = state;
        }

        /// <summary>
        /// Removes an user from the datastore.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        public void DeleteUser(int userId)
        {
            User user;
            if (!this.users.TryRemove(key: userId, value: out user))
            {
                throw new ArgumentOutOfRangeException(paramName: nameof(userId), message: "User not found");
            }
        }

        /// <summary>
        /// Loads an user by its ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>The found <see cref="User"/>.</returns>
        public User LoadUser(int userId)
        {
            User user;
            if (!this.users.TryGetValue(key: userId, value: out user))
            {
                throw new ArgumentOutOfRangeException(paramName: nameof(userId), message: "User not found");
            }

            return user;
        }

        /// <summary>
        /// Loads all users.
        /// </summary>
        /// <returns>The list of <see cref="User"/>.</returns>
        public IReadOnlyCollection<User> LoadAllUsers()
        {
            return this.users.Values.ToArray();
        }

        /// <summary>
        /// Adds a message into the datastore.
        /// </summary>
        /// <param name="message">The <see cref="Message"/>.</param>
        public void AddMessage(Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(paramName: nameof(message));
            }

            this.messages.Enqueue(item: message);
            if (this.messages.Count == MaxMessages)
            {
                // Max messages queue size has been reached.
                // Removes old messages. Removes 10% of messages.
                Task.Run(action: () =>
                {
                    const int MessagesCountMax = MaxMessages - (MaxMessages * 10 / 100);

                    while (this.messages.Count > MessagesCountMax)
                    {
                        Message deletedMessage;
                        this.messages.TryDequeue(result: out deletedMessage);
                    };
                });
            }
        }

        /// <summary>
        /// Loads previous <see cref="Message"/> before the given date.
        /// </summary>
        /// <param name="dateTime">The time limit.</param>
        /// <returns>The list of <see cref="Message"/>.</returns>
        public IReadOnlyCollection<Message> LoadPreviousMessages(DateTime dateTime)
        {
            return this.messages.Where(
                predicate: message => message.DateTime <= dateTime).OrderByDescending(
                keySelector: message => message.DateTime).Take(
                count: MaxMessagesRead).ToArray();
        }
    }
}
