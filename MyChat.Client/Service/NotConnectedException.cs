// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotConnectedException.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    This class defines an exception for not connected user.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Client.Service
{
    using System;

    /// <summary>
    /// This class defines an exception for not connected user.
    /// </summary>
    internal sealed class NotConnectedException : InvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotConnectedException"/> class.
        /// </summary>
        public NotConnectedException()
            : base(message: "missing connection information")
        {
        }
    }
}
