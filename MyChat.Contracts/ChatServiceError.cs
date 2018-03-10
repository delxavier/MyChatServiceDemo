// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChatServiceError.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    Fault context transmitted between chat service and client application.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Contracts
{
    using System;
    using System.Globalization;
    using System.Security;
    using MyChat.Contracts.Properties;

    /// <summary>
    /// Fault context transmitted between chat service and client application.
    /// </summary>
    public sealed class ChatServiceError
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChatServiceError"/> class.
        /// </summary>
        public ChatServiceError()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatServiceError"/> class.
        /// </summary>
        /// <param name="exception">The source exception.</param>
        public ChatServiceError(Exception exception)
        {
            if (exception == null)
            {
                exception = new InvalidOperationException(Resources.UnspecifiedException);
            }

            if (exception is ArgumentException)
            {
                this.Error = ErrorType.InvalidArgument;
            }
            else if (exception is SecurityException)
            {
                this.Error = ErrorType.SecurityError;
            }
            else
            {
                this.Error = ErrorType.UnexpectedError;
            }

            this.ErrorDescription = string.Format(
                CultureInfo.InvariantCulture,
                Resources.UnexpectedErrorFormat,
                exception.GetType().FullName,
                exception.Message);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatServiceError"/> class.
        /// </summary>
        /// <param name="error">The phone device error.</param>
        /// <param name="errorDescription">The description of the error.</param>
        public ChatServiceError(ErrorType error, string errorDescription)
        {
            this.Error = error;
            this.ErrorDescription = errorDescription;
        }

        /// <summary>
        /// Gets or sets the phone device error that produced this fault.
        /// </summary>
        public ErrorType Error { get; set; }

        /// <summary>
        /// Gets or sets the error description of this fault.
        /// </summary>
        public string ErrorDescription { get; set; }
    }
}
