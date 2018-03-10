// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMyChatService.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    This interface describes the chat service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Contracts
{
    using System;
    using System.ServiceModel;

    /// <summary>
    /// This interface describes the chat service.
    /// </summary>
    [ServiceContract(
        Name = "ChatService",
        Namespace = "http://MyChat.Service",
        SessionMode = SessionMode.Required)]
    public interface IMyChatService
    {
        /// <summary>
        /// Opens a session for an user (creates a new user or connects an existing user).
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <returns>The <see cref="ConnectionResult"/>.</returns>
        [OperationContract(IsInitiating = true)]
        [FaultContract(typeof(ChatServiceError))]
        ConnectionResult OpenSession(string userName);

        /// <summary>
        /// Closes a session for an user (disconnects user from chat).
        /// </summary>
        [OperationContract(IsTerminating = true)]
        [FaultContract(typeof(ChatServiceError))]
        void CloseSession();

        /// <summary>
        /// Deletes an user.
        /// </summary>
        [OperationContract(IsTerminating = true)]
        [FaultContract(typeof(ChatServiceError))]
        void LeaveChat();

        /// <summary>
        /// Loads all connected <see cref="UserContract"/>.
        /// </summary>
        /// <returns>The list of connected <see cref="UserContract"/>.</returns>
        [OperationContract(IsInitiating = false, IsTerminating = false)]
        [FaultContract(typeof(ChatServiceError))]
        UserContract[] LoadUsers();

        /// <summary>
        /// Loads an <see cref="UserContract"/> by its ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>The <see cref="UserContract"/>.</returns>
        [OperationContract(IsInitiating = false, IsTerminating = false)]
        [FaultContract(typeof(ChatServiceError))]
        UserContract LoadUser(int userId);

        /// <summary>
        /// Updates the profile of an <see cref="UserContract"/>.
        /// </summary>
        /// <param name="user">The <see cref="UserContract"/>.</param>
        [OperationContract(IsInitiating = false, IsTerminating = false)]
        [FaultContract(typeof(ChatServiceError))]
        void UpdateMyProfile(UserContract user);

        /// <summary>
        /// Loads previous <see cref="MessageContract"/> before the given date.
        /// </summary>
        /// <param name="dateTime">The time limit.</param>
        /// <returns>The list of <see cref="MessageContract"/>.</returns>
        [OperationContract(IsInitiating = false, IsTerminating = false)]
        [FaultContract(typeof(ChatServiceError))]
        MessageContract[] LoadPreviousMessages(DateTime dateTime);

        /// <summary>
        /// Sends a new <see cref="MessageContract"/>.
        /// </summary>
        /// <param name="message">The new <see cref="MessageContract"/>.</param>
        [OperationContract(IsInitiating = false, IsTerminating = false)]
        [FaultContract(typeof(ChatServiceError))]
        void SendMessage(MessageContract message);

        /// <summary>
        /// Starts to write a message.
        /// </summary>
        [OperationContract(IsInitiating = false, IsTerminating = false)]
        [FaultContract(typeof(ChatServiceError))]
        void StartWrite();

        /// <summary>
        /// Cancels to write a message.
        /// </summary>
        [OperationContract(IsInitiating = false, IsTerminating = false)]
        [FaultContract(typeof(ChatServiceError))]
        void CancelWrite();
    }
}
