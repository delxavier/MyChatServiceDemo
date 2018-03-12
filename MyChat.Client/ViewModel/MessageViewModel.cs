// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageViewModel.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    This class defines the view model for message.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Client
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using GalaSoft.MvvmLight;
    using MyChat.Client.Model;
    using MyChat.Client.Properties;

    /// <summary>
    /// This class defines the view model for message.
    /// </summary>
    internal sealed class MessageViewModel : ViewModelBase
    {
        private readonly Message message;
        private readonly User user;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageViewModel"/> class.
        /// </summary>
        /// <param name="message">The <see cref="Message"/> instance.</param>
        /// <param name="user">The <see cref="User"/> instance.</param>
        public MessageViewModel(Message message, User user)
        {
            this.message = message ?? throw new ArgumentNullException(paramName: nameof(message));
            this.user = user ?? throw new ArgumentNullException(paramName: nameof(user));
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        public string Message
        {
            get
            {
                return string.Format(
                    CultureInfo.CurrentCulture, 
                    Resources.MessageFormat, 
                    this.message.DateTime.ToLocalTime().ToString("G", CultureInfo.CurrentCulture), 
                    this.message.Content);
            }
        }

        /// <summary>
        /// Gets the message time.
        /// </summary>
        public DateTime DateTime => this.message.DateTime;

        /// <summary>
        /// Gets the user name.
        /// </summary>
        public string UserName => this.user.UserName;

        /// <summary>
        /// Gets the message content.
        /// </summary>
        public string Content => this.message.Content;

        /// <summary>
        /// Gets a value indicating whether if the user has an image.
        /// </summary>
        public bool HasImage => this.user.Image != null;

        /// <summary>
        /// Gets the user image.
        /// </summary>
        public IReadOnlyCollection<byte> Image
        {
            get { return this.user.Picture; }
        }
    }
}
