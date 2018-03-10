// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDataStore.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    This interface defines the data store.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Service.Model
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// This interface defines the data store.
    /// </summary>
    internal interface IDataStore
    {
        /// <summary>
        /// Adds or updates an user into the datastore.
        /// </summary>
        /// <param name="user">The <see cref="User"/>.</param>
        void AddOrUpdateUser(User user);

        /// <summary>
        /// Updates an user state.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="state">The new <see cref="UserState"/>.</param>
        void UpdateUserState(int userId, UserState state);

        /// <summary>
        /// Removes an user from the datastore.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        void DeleteUser(int userId);

        /// <summary>
        /// Loads an user by its ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>The found <see cref="User"/>.</returns>
        User LoadUser(int userId);

        /// <summary>
        /// Loads all users.
        /// </summary>
        /// <returns>The list of <see cref="User"/>.</returns>
        IReadOnlyCollection<User> LoadAllUsers();

        /// <summary>
        /// Adds a message into the datastore.
        /// </summary>
        /// <param name="message">The <see cref="Message"/>.</param>
        void AddMessage(Message message);

        /// <summary>
        /// Loads previous <see cref="Message"/> before the given date.
        /// </summary>
        /// <param name="dateTime">The time limit.</param>
        /// <returns>The list of <see cref="Message"/>.</returns>
        IReadOnlyCollection<Message> LoadPreviousMessages(DateTime dateTime);
    }
}
