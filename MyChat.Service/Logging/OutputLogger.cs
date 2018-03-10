// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OutputLogger.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    This class defines an <see cref="ILogger"/> which logs in output window.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Service.Logging
{
    using System;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>
    /// This class defines an <see cref="ILogger"/> which logs in output window.
    /// </summary>
    internal sealed class OutputLogger : ILogger
    {
        /// <summary>
        /// Writes a message in the logger with the given <see cref="SeverityLevel"/>.
        /// </summary>
        /// <param name="severity">The <see cref="SeverityLevel"/>.</param>
        /// <param name="trackInfo">The track info to identify log.</param>
        /// <param name="logMessage">The message to write.</param>
        public void Write(SeverityLevel severity, long trackInfo, string logMessage)
        {
            if (logMessage == null)
            {
                throw new ArgumentNullException(paramName: nameof(logMessage));
            }

            this.Write(item: new LogItem(trackInfo: trackInfo, severity: severity, category: string.Empty, message: logMessage, exception: null));
        }

        /// <summary>
        /// Writes a message in the logger with the given <see cref="SeverityLevel"/>.
        /// </summary>
        /// <param name="severity">The <see cref="SeverityLevel"/>.</param>
        /// <param name="logMessage">The message to write.</param>
        public void Write(SeverityLevel severity, string logMessage)
        {
            this.Write(severity: severity, trackInfo: 0L, logMessage: logMessage);
        }

        /// <summary>
        /// Writes a message in the logger with the default <see cref="SeverityLevel"/>.
        /// </summary>
        /// <param name="logMessage">The message to write.</param>
        public void Write(string logMessage)
        {
            this.Write(severity: SeverityLevel.Debug, trackInfo: 0L, logMessage: logMessage);
        }

        /// <summary>
        /// Writes an exception with a description message in the logger with the given <see cref="SeverityLevel"/>.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/>.</param>
        /// <param name="severity">The <see cref="SeverityLevel"/>.</param>
        /// <param name="logMessage">The message to write.</param>
        public void WriteException(Exception exception, SeverityLevel severity, string logMessage)
        {
            this.WriteException(exception: exception, severity: severity, trackInfo: 0L, logMessage: logMessage);
        }

        /// <summary>
        /// Writes an exception with a description message in the logger with the given <see cref="SeverityLevel"/>.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/>.</param>
        /// <param name="trackInfo">The track info to identify log.</param>
        /// <param name="severity">The <see cref="SeverityLevel"/>.</param>
        /// <param name="logMessage">The message to write.</param>
        public void WriteException(Exception exception, long trackInfo, SeverityLevel severity, string logMessage)
        {
            if (logMessage == null)
            {
                throw new ArgumentNullException(paramName: nameof(logMessage));
            }

            if (exception == null)
            {
                throw new ArgumentNullException(paramName: nameof(exception));
            }

            this.Write(item: new LogItem(trackInfo: trackInfo, severity: severity, category: string.Empty, message: logMessage, exception: null));
        }

        /// <summary>
        /// Formats a message in the logger with the given <see cref="SeverityLevel"/>.
        /// </summary>
        /// <param name="severity">The <see cref="SeverityLevel"/>.</param>
        /// <param name="trackInfo">The track info to identify log.</param>
        /// <param name="logMessageFormat">The message to write.</param>
        /// <param name="args">The list of args.</param>
        public void Format(SeverityLevel severity, long trackInfo, string logMessageFormat, params object[] args)
        {
            if (logMessageFormat == null)
            {
                throw new ArgumentNullException(paramName: nameof(logMessageFormat));
            }

            string logMessage = string.Format(provider: CultureInfo.InvariantCulture, format: logMessageFormat, args: args);
            if (!string.IsNullOrEmpty(value: logMessage))
            {
                this.Write(severity: severity, trackInfo: trackInfo, logMessage: logMessage);
            }
        }

        /// <summary>
        /// Formats a message in the logger with the given <see cref="SeverityLevel"/>.
        /// </summary>
        /// <param name="severity">The <see cref="SeverityLevel"/>.</param>
        /// <param name="logMessageFormat">The message to write.</param>
        /// <param name="args">The list of args.</param>
        public void Format(SeverityLevel severity, string logMessageFormat, params object[] args)
        {
            this.Format(severity: severity, trackInfo: 0L, logMessageFormat: logMessageFormat, args: args);
        }

        /// <summary>
        /// Formats a message in the logger with the default <see cref="SeverityLevel"/>.
        /// </summary>
        /// <param name="logMessageFormat">The message to write.</param>
        /// <param name="args">The list of args.</param>
        public void Format(string logMessageFormat, params object[] args)
        {
            this.Format(severity: SeverityLevel.Debug, trackInfo: 0L, logMessageFormat: logMessageFormat, args: args);
        }

        /// <summary>
        /// Formats an exception with a description message in the logger with the given <see cref="SeverityLevel"/>.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/>.</param>
        /// <param name="severity">The <see cref="SeverityLevel"/>.</param>
        /// <param name="logMessageFormat">The message to write.</param>
        /// <param name="args">The list of args.</param>
        public void FormatException(Exception exception, SeverityLevel severity, string logMessageFormat, params object[] args)
        {
            this.FormatException(exception: exception, trackInfo: 0L, severity: severity, logMessageFormat: logMessageFormat, args: args);
        }

        /// <summary>
        /// Formats an exception with a description message in the logger with the given <see cref="SeverityLevel"/>.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/>.</param>
        /// <param name="trackInfo">The track info to identify log.</param>
        /// <param name="severity">The <see cref="SeverityLevel"/>.</param>
        /// <param name="logMessageFormat">The message to write.</param>
        /// <param name="args">The list of args.</param>
        public void FormatException(Exception exception, long trackInfo, SeverityLevel severity, string logMessageFormat, params object[] args)
        {
            string logMessage = string.Format(provider: CultureInfo.InvariantCulture, format: logMessageFormat, args: args);
            if (!string.IsNullOrEmpty(value: logMessage))
            {
                this.WriteException(exception: exception, severity: severity, trackInfo: trackInfo, logMessage: logMessage);
            }
        }

        /// <summary>
        /// Writes a <see cref="LogItem"/>.
        /// </summary>
        /// <param name="item">The <see cref="LogItem"/>.</param>
        private void Write(LogItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(paramName: nameof(item));
            }

            string message = string.Format(
                CultureInfo.InvariantCulture,
                "{0}|{1}|{2}|{3}|{4}",
                item.Timestamp.ToString(format: "yyyyMMdd/HH:mm:ss:FFF", provider: CultureInfo.InvariantCulture),
                item.Severity,
                item.Category,
                item.Message,
                item.Exception);
            Trace.WriteLine(message: message);
        }

        /// <summary>
        /// This class defines a log item.
        /// </summary>
        private sealed class LogItem
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LogItem"/> class.
            /// </summary>
            /// <param name="trackInfo">The log track info.</param>
            /// <param name="severity">The log <see cref="SeverityLevel"/>.</param>
            /// <param name="category">The log category.</param>
            /// <param name="message">The log message.</param>
            /// <param name="exception">The log <see cref="Exception"/>.</param>
            public LogItem(long trackInfo, SeverityLevel severity, string category, string message, Exception exception)
            {
                this.TrackInfo = trackInfo;
                this.Severity = severity;
                this.Category = category ?? string.Empty; ;
                this.Message = message ?? throw new ArgumentNullException(paramName: nameof(message));
                this.Exception = exception;
                this.Timestamp = DateTime.UtcNow;
            }

            /// <summary>
            /// Gets the log <see cref="SeverityLevel"/>.
            /// </summary>
            public SeverityLevel Severity { get; }

            /// <summary>
            /// Gets the log message.
            /// </summary>
            public string Message { get; }

            /// <summary>
            /// Gets the log category.
            /// </summary>
            public string Category { get; }

            /// <summary>
            /// Gets the log track info.
            /// </summary>
            public long TrackInfo { get; }

            /// <summary>
            /// Gets the log time.
            /// </summary>
            public DateTime Timestamp { get; }

            /// <summary>
            /// Gets the log <see cref="Exception"/>.
            /// </summary>
            public Exception Exception { get; }
        }
    }
}