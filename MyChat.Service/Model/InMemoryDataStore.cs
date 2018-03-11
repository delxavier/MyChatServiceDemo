﻿// --------------------------------------------------------------------------------------------------------------------
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
        /// Adds or updates an user into the data store.
        /// </summary>
        /// <param name="user">The <see cref="User"/>.</param>
        /// <returns>The user ID.</returns>
        public int AddOrUpdateUser(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(paramName: nameof(user));
            }

            if (string.IsNullOrEmpty(value: user.UserName))
            {
                throw new ArgumentNullException(paramName: nameof(user), message: "user name can't be null");
            }

            if (user.UserId == 0)
            {
                User knownUser = this.users.Values.SingleOrDefault(
                    predicate: item => string.Equals(a: user.UserName, b: item.UserName, comparisonType: StringComparison.OrdinalIgnoreCase));

                if (knownUser != null)
                {
                    return knownUser.UserId;
                }

                int userId = this.users.Keys.Max() + 1;
                var newUser = new User(userId: userId) { Picture = user.Picture, State = UserState.New, UserName = user.UserName };
                this.users.AddOrUpdate(key: newUser.UserId, addValue: newUser, updateValueFactory: (key, oldValue) => newUser);
                return userId;
            }

            this.users.AddOrUpdate(key: user.UserId, addValue: user, updateValueFactory: (key, oldValue) => user);
            return user.UserId;
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
        /// Removes an user from the data store.
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
        /// Loads an user by its name.
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <returns>The found <see cref="User"/>.</returns>
        public User LoadUser(string userName)
        {
            if (string.IsNullOrEmpty(value: userName))
            {
                throw new ArgumentNullException(paramName: nameof(userName));
            }

            return this.users.Values.Single(predicate: user => string.Equals(a: userName, b: user.UserName, comparisonType: StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Checks if an user with this name exists.
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <returns>True if an user is found otherwise false.</returns>
        public bool UserExist(string userName)
        {
            if (string.IsNullOrEmpty(value: userName))
            {
                throw new ArgumentNullException(paramName: nameof(userName));
            }

            return this.users.Values.SingleOrDefault(predicate: user => string.Equals(a: userName, b: user.UserName, comparisonType: StringComparison.OrdinalIgnoreCase)) != null;
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
        /// Adds a message into the data store.
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
