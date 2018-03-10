// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConnectionResult.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    This class defines the result of an user connection.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Contracts
{
    /// <summary>
    /// This class defines the result of an user connection.
    /// </summary>
    public sealed class ConnectionResult
    {
        /// <summary>
        /// Gets or sets the user ID.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the authentication key for this user.
        /// </summary>
        public string AuthenticationKey { get; set; }
    }
}
