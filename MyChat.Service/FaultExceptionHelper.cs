//-----------------------------------------------------------------------
// <copyright file="FaultExceptionHelper.cs">
//     Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//     Helps creation of fault exceptions used in WCF between server and consumer.
// </summary>
//-----------------------------------------------------------------------

namespace MyChat.Service
{
    using System;
    using System.Globalization;
    using System.ServiceModel;
    using MyChat.Contracts;
    using MyChat.Service.Properties;

    /// <summary>
    /// Helps creation of fault exceptions used in WCF between server and consumer.
    /// </summary>
    internal static class FaultExceptionHelper
    {
        /// <summary>
        /// Creates a new <see cref="FaultException{ChatServiceError}"/> from an existing <see cref="Exception"/>.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/> which generates this error.</param>
        /// <returns>A new <see cref="FaultException{ChatServiceError}"/>.</returns>
        public static FaultException<ChatServiceError> From(Exception exception)
        {
            if (exception == null)
            {
                exception = new InvalidOperationException(message: Resources.UnspecifiedException);
            }

            ErrorType error = ErrorType.UnexpectedError;
            if (exception is ArgumentException)
            {
                error = ErrorType.InvalidArgument;
            }

            string errorDescription = string.Format(
                CultureInfo.InvariantCulture,
                Resources.UnexpectedErrorFormat,
                exception.GetType().FullName,
                exception.Message);
            var fault = new ChatServiceError(error: error, errorDescription: errorDescription);
            return new FaultException<ChatServiceError>(detail: fault, reason: fault.ErrorDescription);
        }

        /// <summary>
        /// Creates a new <see cref="FaultException{ChatServiceError}"/> from an existing <see cref="Exception"/>.
        /// </summary>
        /// <param name="error">The <see cref="ErrorType"/>.</param>
        /// <param name="exception">The <see cref="Exception"/> which generates this error.</param>
        /// <returns>A new <see cref="FaultException{ChatServiceError}"/>.</returns>
        public static FaultException<ChatServiceError> From(ErrorType error, Exception exception)
        {
            if (exception == null)
            {
                exception = new InvalidOperationException(message: Resources.UnspecifiedException);
            }

            string errorDescription = string.Format(
                CultureInfo.InvariantCulture,
                Resources.UnexpectedErrorFormat,
                exception.GetType().FullName,
                exception.Message);
            var fault = new ChatServiceError(error: error, errorDescription: errorDescription);
            return new FaultException<ChatServiceError>(detail: fault, reason: fault.ErrorDescription);
        }
    }
}
