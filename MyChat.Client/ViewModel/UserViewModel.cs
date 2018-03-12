// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserViewModel.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    This class defines the view model for an user.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Client
{
    using System;
    using GalaSoft.MvvmLight;
    using MyChat.Client.Model;

    /// <summary>
    /// This class defines the view model for an user.
    /// </summary>
    internal sealed class UserViewModel : ViewModelBase
    {
        private readonly User user;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserViewModel"/> class.
        /// </summary>
        /// <param name="user">The <see cref="User"/> instance.</param>
        public UserViewModel(User user)
        {
            this.user = user ?? throw new ArgumentNullException(paramName: nameof(user));
        }

        /// <summary>
        /// Gets the user.
        /// </summary>
        public User User => this.user;

        /// <summary>
        /// Gets the user name.
        /// </summary>
        public string UserName => this.user.UserName;

        /// <summary>
        /// Gets a value indicating whether if the user is writing.
        /// </summary>
        public bool IsWriting => this.user.State == UserState.Writing;

        /// <summary>
        /// Gets or sets the user state.
        /// </summary>
        public UserState State
        {
            get { return this.user.State; }
            set
            {
                if (this.user.State != value)
                {
                    this.user.State = value;
                    this.RaisePropertyChanged(() => this.State);
                    this.RaisePropertyChanged(() => this.IsWriting);
                }
            }
        }
    }
}
