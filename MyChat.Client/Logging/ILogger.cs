// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILogger.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    This interface defines a logger.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Client.Logging
{
    using System;

    /// <summary>
    /// This interface defines a logger.
    /// </summary>
    internal interface ILogger
    {
        /// <summary>
        /// Writes a message in the logger with the given <see cref="SeverityLevel"/>.
        /// </summary>
        /// <param name="severity">The <see cref="SeverityLevel"/>.</param>
        /// <param name="trackInfo">The track info to identify log.</param>
        /// <param name="logMessage">The message to write.</param>
        void Write(SeverityLevel severity, long trackInfo, string logMessage);

        /// <summary>
        /// Writes a message in the logger with the given <see cref="SeverityLevel"/>.
        /// </summary>
        /// <param name="severity">The <see cref="SeverityLevel"/>.</param>
        /// <param name="logMessage">The message to write.</param>
        void Write(SeverityLevel severity, string logMessage);

        /// <summary>
        /// Writes a message in the logger with the default <see cref="SeverityLevel"/>.
        /// </summary>
        /// <param name="logMessage">The message to write.</param>
        void Write(string logMessage);

        /// <summary>
        /// Writes an exception with a description message in the logger with the given <see cref="SeverityLevel"/>.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/>.</param>
        /// <param name="severity">The <see cref="SeverityLevel"/>.</param>
        /// <param name="logMessage">The message to write.</param>
        void WriteException(Exception exception, SeverityLevel severity, string logMessage);

        /// <summary>
        /// Writes an exception with a description message in the logger with the given <see cref="SeverityLevel"/>.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/>.</param>
        /// <param name="trackInfo">The track info to identify log.</param>
        /// <param name="severity">The <see cref="SeverityLevel"/>.</param>
        /// <param name="logMessage">The message to write.</param>
        void WriteException(Exception exception, long trackInfo, SeverityLevel severity, string logMessage);

        /// <summary>
        /// Formats a message in the logger with the given <see cref="SeverityLevel"/>.
        /// </summary>
        /// <param name="severity">The <see cref="SeverityLevel"/>.</param>
        /// <param name="trackInfo">The track info to identify log.</param>
        /// <param name="logMessageFormat">The message to write.</param>
        /// <param name="args">The list of args.</param>
        void Format(SeverityLevel severity, long trackInfo, string logMessageFormat, params object[] args);

        /// <summary>
        /// Formats a message in the logger with the given <see cref="SeverityLevel"/>.
        /// </summary>
        /// <param name="severity">The <see cref="SeverityLevel"/>.</param>
        /// <param name="logMessageFormat">The message to write.</param>
        /// <param name="args">The list of args.</param>
        void Format(SeverityLevel severity, string logMessageFormat, params object[] args);

        /// <summary>
        /// Formats a message in the logger with the default <see cref="SeverityLevel"/>.
        /// </summary>
        /// <param name="logMessageFormat">The message to write.</param>
        /// <param name="args">The list of args.</param>
        void Format(string logMessageFormat, params object[] args);

        /// <summary>
        /// Formats an exception with a description message in the logger with the given <see cref="SeverityLevel"/>.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/>.</param>
        /// <param name="severity">The <see cref="SeverityLevel"/>.</param>
        /// <param name="logMessageFormat">The message to write.</param>
        /// <param name="args">The list of args.</param>
        void FormatException(Exception exception, SeverityLevel severity, string logMessageFormat, params object[] args);

        /// <summary>
        /// Formats an exception with a description message in the logger with the given <see cref="SeverityLevel"/>.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/>.</param>
        /// <param name="trackInfo">The track info to identify log.</param>
        /// <param name="severity">The <see cref="SeverityLevel"/>.</param>
        /// <param name="logMessageFormat">The message to write.</param>
        /// <param name="args">The list of args.</param>
        void FormatException(Exception exception, long trackInfo, SeverityLevel severity, string logMessageFormat, params object[] args);
    }
}
