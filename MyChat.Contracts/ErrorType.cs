// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ErrorType.cs"
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    Describes the error met on the chat service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Contracts
{
    /// <summary>
    /// Describes the error met on the chat service.
    /// </summary>
    public enum ErrorType
    {
        /// <summary>
        /// No error has been detected.
        /// </summary>
        NoError,

        /// <summary>
        /// A database error has been trapped.
        /// </summary>
        DatabaseError,

        /// <summary>
        /// An invalid argument has been provided.
        /// </summary>
        InvalidArgument,

        /// <summary>
        /// User is not known.
        /// </summary>
        UnknownUser,

        /// <summary>
        /// A security error.
        /// </summary>
        SecurityError,

        /// <summary>
        /// Unhandled error has been trapped.
        /// </summary>
        UnexpectedError,

        /// <summary>
        /// Error handled by unspecified.
        /// </summary>
        UnspecifiedError
    }
}
