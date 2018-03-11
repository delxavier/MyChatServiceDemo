// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserExtender.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    This class contains method extender for <see cref="User"/>.
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
    /// This class contains method extender for <see cref="User"/>.
    /// </summary>
    internal static class UserExtender
    {
        /// <summary>
        /// Converts an <see cref="User"/> to an <see cref="UserContract"/>.
        /// </summary>
        /// <param name="user">The <see cref="User"/> to convert.</param>
        /// <returns>The <see cref="UserContract"/>.</returns>
        public static UserContract ToContract(this User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(paramName: nameof(user));
            }

            if (string.IsNullOrEmpty(value: user.UserName))
            {
                throw new ArgumentNullException(paramName: nameof(user), message: "user name can't be null");
            }

            return new UserContract
            {
                UserName = user.UserName,
                Picture = user.Picture.ToArray(),
                State = (Contracts.UserState)(int)user.State,
                UserId = user.UserId
            };
        }

        /// <summary>
        /// Converts a list of <see cref="User"/> to a list of <see cref="UserContract"/>.
        /// </summary>
        /// <param name="users">The list of <see cref="User"/> to convert.</param>
        /// <returns>The list of <see cref="UserContract"/> converted.</returns>
        public static UserContract[] ToContracts(this IReadOnlyCollection<User> users)
        {
            if (users == null)
            {
                throw new ArgumentNullException(paramName: nameof(users));
            }

            return users.Where(predicate: user => user != null).Select(selector: user => user.ToContract()).ToArray();
        }

        /// <summary>
        /// Converts an <see cref="UserContract"/> to an <see cref="User"/>.
        /// </summary>
        /// <param name="contract">The <see cref="UserContract"/> to convert.</param>
        /// <returns>The <see cref="User"/>.</returns>
        public static User FromContract(this UserContract contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(paramName: nameof(contract));
            }

            if (string.IsNullOrEmpty(value: contract.UserName))
            {
                throw new ArgumentNullException(paramName: nameof(contract), message: "user name can't be null");
            }

            return new User(userId: contract.UserId)
            {
                UserName = contract.UserName,
                Picture = contract.Picture.ToArray(),
                State = (Model.UserState)(int)contract.State
            };
        }
    }
}
