// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserContract.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    This class defines an user.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Contracts
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// This class defines an user.
    /// </summary>
    public sealed class UserContract
    {
        /// <summary>
        /// Gets or sets the user ID.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the user picture.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "OK here in contract")]
        public byte[] Picture { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="UserState"/>.
        /// </summary>
        public UserState State { get; set; }
    }
}
