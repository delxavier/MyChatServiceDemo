// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SeverityLevel.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    This enumeration contains level for logging.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Client.Logging
{
    /// <summary>
    /// This enumeration contains level for logging.
    /// </summary>
    internal enum SeverityLevel
    {
        /// <summary>
        /// Debug level.
        /// </summary>
        Debug,

        /// <summary>
        /// Info level.
        /// </summary>
        Info,

        /// <summary>
        /// Warning level.
        /// </summary>
        Warning,

        /// <summary>
        /// Error level.
        /// </summary>
        Error,

        /// <summary>
        /// Failure level.
        /// </summary>
        Failure
    }
}