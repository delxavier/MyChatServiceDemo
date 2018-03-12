using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyChat.Client;
namespace MyChat.Tests
{
    using MyChat.Client.Logging;
    using MyChat.Client.Model;
    using MyChat.Client.Properties;
    using MyChat.Client.Service;
    using MyChat.Contracts;

    [TestClass]
    public class UnitTestClient
    {
        [TestMethod]
        public void TestMethodConnectSucceed()
        {
            try
            {
                Task.Run(async () =>
                {
                    var viewModel = new MainViewModel(new OutputLogger(), new CommunicationManager());
                    viewModel.UserName = "test";
                    await viewModel.ConnectAsync();
                    Assert.IsTrue(!viewModel.HasError && viewModel.Connected);
                }).Wait();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(format: "Can't connect {0}", args: exception);
                throw;
            }
        }

        [TestMethod]
        public void TestMethodAlreadyConnect()
        {
            try
            {
                Task.Run(async () =>
                {
                    var viewModel = new MainViewModel(new OutputLogger(), new CommunicationManager());
                    viewModel.UserName = "alreadyConnect";
                    await viewModel.ConnectAsync();
                    Assert.IsTrue(viewModel.HasError && !viewModel.Connected && viewModel.ErrorMessage == Resources.AlreadyConnectUser);
                }).Wait();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(format: "Can't connect {0}", args: exception);
                throw;
            }
        }

        [TestMethod]
        public void TestMethodSendMessage()
        {
            try
            {
                Task.Run(async () =>
                {
                    var viewModel = new MainViewModel(new OutputLogger(), new CommunicationManager());
                    viewModel.UserName = "test";
                    await viewModel.ConnectAsync();
                    string text = "test message";
                    viewModel.Message = text;
                    viewModel.Message += Environment.NewLine;
                    Assert.IsTrue(viewModel.Messages.Count == 1);
                    var message = viewModel.Messages[0];
                    Assert.IsTrue(viewModel.Message == null);
                    Assert.IsTrue(string.Equals(message.Content, text));
                }).Wait();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(format: "Can't connect {0}", args: exception);
                throw;
            }
        }

        [TestMethod]
        public void TestMethodWriteStatus()
        {
            try
            {
                Task.Run(async () =>
                {
                    var viewModel = new MainViewModel(new OutputLogger(), new CommunicationManager());
                    viewModel.UserName = "test";
                    await viewModel.ConnectAsync();
                    viewModel.Message = "test message";
                    Assert.IsTrue(viewModel.Messages.Count == 0);
                    var user = viewModel.Users[0];
                    Assert.IsTrue(user.IsWriting);
                    viewModel.Message += Environment.NewLine;
                    user = viewModel.Users[0];
                    Assert.IsTrue(!user.IsWriting);
                }).Wait();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(format: "Can't connect {0}", args: exception);
                throw;
            }
        }

        [TestMethod]
        public void TestMethodUpdateName()
        {
            try
            {
                Task.Run(async () =>
                {
                    var viewModel = new MainViewModel(new OutputLogger(), new CommunicationManager());
                    viewModel.UserName = "test";
                    await viewModel.ConnectAsync();
                    viewModel.UserName = "new name" + Environment.NewLine;
                    var user = viewModel.Users[0];
                    Assert.IsTrue(string.Equals(user.UserName, viewModel.UserName));
                }).Wait();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(format: "Can't connect {0}", args: exception);
                throw;
            }
        }

        private sealed class CommunicationManager : ICommunicationManager
        {
            private MyChat.Client.Model.UserState state;
            string name;
            public bool ConnectionOpened
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public User CurrentUser
            {
                get
                {
                    return new User(1) { UserName = this.name };
                }
            }

            public event EventHandler<MessageEventArgs> OnNewMessage;
            public event EventHandler<UserUpdateEventArgs> OnUserUpdate;

            public void Close()
            {
                throw new NotImplementedException();
            }

            public Task CloseAsync()
            {
                throw new NotImplementedException();
            }

            public async Task ConnectAsync(string userName)
            {
                if (userName == "alreadyConnect")
                {
                    throw Service.FaultExceptionHelper.From(ErrorType.AlreadyConnectedUser, null);
                }

                this.name = userName;
            }

            public Task<IReadOnlyCollection<Message>> LoadPreviousMessages(DateTime dateTime)
            {
                throw new NotImplementedException();
            }

            public async Task<User> LoadUserAsync(int userId)
            {
                return new User(1) { UserName = this.name, State = this.state };
            }

            public async Task<IReadOnlyCollection<User>> LoadUsersAsync()
            {
                return new[] { new User(1) { UserName = this.name } };
            }

            public async Task SendMessageAsync(string message)
            {
                this.OnNewMessage?.Invoke(this, new MessageEventArgs(new Message(1, message, DateTime.UtcNow)));
            }

            public async Task UpdateMyProfileAsync(User user)
            {
                this.name = user.UserName;
                this.OnUserUpdate(this, new UserUpdateEventArgs(1));
            }

            public async Task UpdateMyStatusAsync(MyChat.Client.Model.UserState state)
            {
                this.state = state;
                this.OnUserUpdate(this, new UserUpdateEventArgs(1, state));
            }
        }
    }
}
