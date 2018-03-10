// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConnectionManager.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    This is an implementation of the <see cref="IConnectionManager"/>.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Service.ConnectionManager
{
    using System.Net;
    using MyChat.Service.Logging;

    /// <summary>
    /// This is an implementation of the <see cref="IConnectionManager"/>.
    /// </summary>
    internal sealed class ConnectionManager : IConnectionManager
    {
        /// <summary> The <see cref="ILogger"/> instance. </summary>
        private readonly ILogger logger;

        /// <summary> The <see cref="HttpListener"/> instance. </summary>
        private readonly HttpListener listener = new HttpListener();

        public ConnectionManager(ILogger logger)
        { }
    }
}
