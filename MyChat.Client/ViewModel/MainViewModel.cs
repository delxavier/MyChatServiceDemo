// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainViewModel.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    This class defines the view model for the main view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Client
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.ServiceModel;
    using System.Threading.Tasks;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;
    using Microsoft.Win32;
    using MyChat.Client.Logging;
    using MyChat.Client.Properties;
    using MyChat.Client.Service;
    using MyChat.Contracts;

    /// <summary>
    /// This class defines the view model for the main view.
    /// </summary>
    internal sealed class MainViewModel : ViewModelBase
    {
        private const int MaxMessageCount = 10;
        private readonly ICommunicationManager communicationManager;
        private readonly ILogger logger;
        private readonly ConcurrentQueue<MessageViewModel> messages = new ConcurrentQueue<MessageViewModel>();
        private readonly ConcurrentDictionary<int, UserViewModel> users = new ConcurrentDictionary<int, UserViewModel>();
        private string message;
        private RelayCommand connectCommand;
        private RelayCommand changeImageCommand;
        private volatile bool connectionInProgress;
        private volatile bool writeInProgress;
        private bool connected;
        private bool hasError;
        private string errorMessage;
        private string userName;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        public MainViewModel(ILogger logger, ICommunicationManager communicationManager)
        {
            this.logger = logger ?? throw new ArgumentNullException(paramName: nameof(logger));
            this.communicationManager = communicationManager ?? throw new ArgumentNullException(paramName: nameof(communicationManager));

            this.communicationManager.OnNewMessage += (s, e) =>
            {
                if (this.connected)
                {
                    while (this.messages.Count >= MaxMessageCount)
                    {
                        this.messages.TryDequeue(out var message);
                    }

                    UserViewModel user;
                    if (this.users.TryGetValue(e.Message.OwnerId, out user))
                    {
                        this.messages.Enqueue(new MessageViewModel(e.Message, user.User));
                        this.RaisePropertyChanged(() => this.Messages);
                    }
                }
            };

            this.communicationManager.OnUserUpdate += async (s, e) =>
            {
                if (this.connected)
                {
                    if (e.FullUpdate || !this.users.ContainsKey(e.UserId))
                    {
                        this.users[key: e.UserId] = new UserViewModel(await this.communicationManager.LoadUserAsync(e.UserId));
                        this.RaisePropertyChanged(() => this.UserName);
                        this.RaisePropertyChanged(() => this.Image);
                        this.RaisePropertyChanged(() => this.HasImage);
                        this.RaisePropertyChanged(() => this.NeedImage);
                    }
                    else if (this.users.ContainsKey(e.UserId))
                    {
                        if (e.State == Model.UserState.Deleted)
                        {
                            this.users.TryRemove(e.UserId, out var user);
                        }
                        else
                        {
                            this.users[e.UserId].State = e.State;
                        }
                    }

                    this.RaisePropertyChanged(() => this.Users);
                }
            };
        }

        /// <summary>
        /// Gets the connect command.
        /// </summary>
        public RelayCommand ConnectCommand
        {
            get { return this.connectCommand ?? (this.connectCommand = new RelayCommand(execute: async () => await this.ConnectAsync())); }
        }

        /// <summary>
        /// Gets the change image command.
        /// </summary>
        public RelayCommand ChangeImageCommand
        {
            get { return this.changeImageCommand ?? (this.changeImageCommand = new RelayCommand(execute: () => this.ChangeImage())); }
        }

        /// <summary>
        /// Gets the list of message.
        /// </summary>
        public ObservableCollection<MessageViewModel> Messages
        {
            get { return new ObservableCollection<MessageViewModel>(collection: this.messages.OrderBy(keySelector: message => message.DateTime)); }
        }

        /// <summary>
        /// Gets the list of users.
        /// </summary>
        public ObservableCollection<UserViewModel> Users
        {
            get { return new ObservableCollection<UserViewModel>(collection: this.users.Values.OrderBy(keySelector: user => user.UserName)); }
        }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string UserName
        {
            get { return this.userName?.Trim(); }
            set
            {
                if (this.Set(ref this.userName, value))
                {
                    if (!string.IsNullOrEmpty(value) && this.Connected && value.EndsWith(value: Environment.NewLine))
                    {
                        new Action(async () =>
                        {
                            try
                            {
                                var user = this.communicationManager.CurrentUser;
                                user.UserName = this.userName.Trim();
                                await this.communicationManager.UpdateMyProfileAsync(user);
                                this.HasError = false;
                            }
                            catch (FaultException<ChatServiceError> exception)
                            {
                                this.logger.FormatException(exception, SeverityLevel.Failure, "can't update profile {0}", exception.Detail.ErrorDescription);
                                this.ErrorMessage = exception.Detail.Error == ErrorType.AlreadyConnectedUser ? Resources.AlreadyConnectUser : Resources.UpdateError;
                                this.HasError = true;
                            }
                            catch (Exception exception)
                            {
                                this.logger.WriteException(exception, SeverityLevel.Failure, "can't update profile");
                                this.ErrorMessage = Resources.UpdateError;
                                this.HasError = true;
                            }
                        }).Invoke();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the user image.
        /// </summary>
        public IReadOnlyCollection<byte> Image
        {
            get { return this.communicationManager.CurrentUser?.Picture; }
        }

        /// <summary>
        /// Gets a value indicating whether if the user has an image.
        /// </summary>
        public bool HasImage
        {
            get { return this.communicationManager.CurrentUser?.Image != null; }
        }

        /// <summary>
        /// Gets a value indicating whether if the user has an image.
        /// </summary>
        public bool NeedImage
        {
            get { return !this.HasImage; }
        }

        /// <summary>
        /// Gets a value indicating the connection state.
        /// </summary>
        public bool Connected
        {
            get { return this.connected; }
            private set
            {
                this.Set(field: ref this.connected, newValue: value);
            }
        }

        /// <summary>
        /// Gets a value indicating whether if the connect command can be executed.
        /// </summary>
        public bool CanConnect
        {
            get
            {
                return !this.connectionInProgress
                && ((!string.IsNullOrWhiteSpace(value: this.userName) && !this.connected)
                || this.connected);
            }
        }

        /// <summary>
        /// Gets a value indicating whether if there are an error.
        /// </summary>
        public bool HasError
        {
            get { return this.hasError; }
            private set
            {
                this.Set(field: ref this.hasError, newValue: value);
            }
        }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string ErrorMessage
        {
            get { return this.errorMessage; }
            private set
            {
                this.Set(field: ref this.errorMessage, newValue: value);
            }
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message
        {
            get
            {
                return this.message;
            }

            set
            {
                if (this.Set(ref this.message, value))
                {
                    if (string.IsNullOrEmpty(value) && this.Connected)
                    {
                        this.writeInProgress = false;
                        new Action(async () =>
                        {
                            try
                            {
                                await this.communicationManager.UpdateMyStatusAsync(Model.UserState.Online);
                            }
                            catch (Exception exception)
                            {
                                this.logger.WriteException(exception, SeverityLevel.Failure, "can't update status");
                            }
                        }).Invoke();
                    }
                    else if (!string.IsNullOrEmpty(value) && this.Connected && !this.writeInProgress)
                    {
                        this.writeInProgress = true;
                        new Action(async () =>
                        {
                            try
                            {
                                await this.communicationManager.UpdateMyStatusAsync(Model.UserState.Writing);
                            }
                            catch (Exception exception)
                            {
                                this.logger.WriteException(exception, SeverityLevel.Failure, "can't update status");
                            }                            
                        }).Invoke();
                    }
                    else if (value.EndsWith(value: Environment.NewLine) && this.Connected)
                    {
                        new Action(async () =>
                        {
                            try
                            {
                                await this.communicationManager.SendMessageAsync(message: this.message.Trim());
                                this.Message = null;
                            }
                            catch (Exception exception)
                            {
                                this.logger.WriteException(exception, SeverityLevel.Failure, "can't send message to server");
                            }
                        }).Invoke();
                    }
                }
            }
        }

        /// <summary>
        /// Cleans view model.
        /// </summary>
        public override void Cleanup()
        {
            if (this.connected)
            {
                this.communicationManager.Close();
            }

            base.Cleanup();
        }

        internal void ChangeImage()
        {
            if (this.connected)
            {
                var fileDialog = new OpenFileDialog();
                fileDialog.CheckFileExists = true;
                fileDialog.Multiselect = false;
                fileDialog.Filter = "Images | *.png;*.jpg";

                if (fileDialog.ShowDialog() == true)
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            var user = this.communicationManager.CurrentUser;
                            user.Picture = File.ReadAllBytes(fileDialog.FileName);
                            await this.communicationManager.UpdateMyProfileAsync(user);
                        }
                        catch (Exception exception)
                        {
                            this.logger.WriteException(exception, SeverityLevel.Failure, "can't update profile");
                        }
                    });
                }
            }
        }

        internal async Task ConnectAsync()
        {
            if (this.CanConnect)
            {
                if (!this.connected)
                {
                    try
                    {
                        this.connectionInProgress = true;
                        this.users.Clear();
                        await this.communicationManager.ConnectAsync(userName: this.userName);
                        var connectedUsers = await this.communicationManager.LoadUsersAsync();
                        foreach (var user in connectedUsers)
                        {
                            this.users.TryAdd(user.UserId, new UserViewModel(user));
                        }

                        this.RaisePropertyChanged(() => this.Users);
                        this.RaisePropertyChanged(() => this.HasImage);
                        this.RaisePropertyChanged(() => this.NeedImage);
                        this.RaisePropertyChanged(() => this.Image);
                        this.Connected = true;
                        this.HasError = false;
                        this.ErrorMessage = null;
                    }
                    catch (FaultException<ChatServiceError> exception)
                    {
                        this.logger.FormatException(exception, SeverityLevel.Failure, "can't connect to server {0}", exception.Detail.ErrorDescription);
                        this.ErrorMessage = exception.Detail.Error == ErrorType.AlreadyConnectedUser ? Resources.AlreadyConnectUser : Resources.ConnectionError;
                        this.HasError = true;
                    }
                    catch (Exception exception)
                    {
                        this.logger.WriteException(exception, SeverityLevel.Failure, "can't connect to server");
                        this.ErrorMessage = Resources.ConnectionError;
                        this.HasError = true;
                    }
                    finally
                    {
                        this.connectionInProgress = false;
                    }
                }
                else
                {
                    try
                    {
                        await this.communicationManager.CloseAsync();
                        this.Connected = false;
                        this.users.Clear();
                        this.RaisePropertyChanged(() => this.Users);
                    }
                    catch (Exception exception)
                    {
                        this.logger.WriteException(exception, SeverityLevel.Failure, "can't disconnect from server");
                    }
                }
            }
        }        
    }
}
