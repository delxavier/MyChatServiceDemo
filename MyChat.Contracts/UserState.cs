// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserState.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    This is the list of all available value of user state.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Contracts
{
    /// <summary>
    /// This is the list of all available value of user state.
    /// </summary>
    public enum UserState
    {
        /// <summary>
        /// This is a new user.
        /// </summary>
        New = 0,

        /// <summary>
        /// The user is online.
        /// </summary>
        Online,

        /// <summary>
        /// The user is idle.
        /// </summary>
        Idle,

        /// <summary>
        /// The user is offline.
        /// </summary>
        Offline,

        /// <summary>
        /// The user user is deleted.
        /// </summary>
        Deleted,

        /// <summary>
        /// The user writes a message.
        /// </summary>
        Writing
    }
}
