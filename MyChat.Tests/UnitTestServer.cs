using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyChat.Service.Model;

namespace MyChat.Tests
{
    [TestClass]
    public class UnitTestServer
    {
        [TestMethod]
        public void TestMethodAddUserSucceed()
        {
            try
            {
                var dataStore = new InMemoryDataStore();
                var user = new User { UserName = "Test1" };
                int id = dataStore.AddOrUpdateUser(user);
                Assert.IsTrue(id > 0);
                var loadUser = dataStore.LoadUser(id);
                Assert.IsTrue(loadUser.UserId == id && string.Equals(user.UserName, loadUser.UserName), "Load user is not the same");
            }
            catch (Exception exception)
            {
                Debug.WriteLine(format: "Can't add user {0}", args: exception);
                throw;
            }
        }

        [TestMethod]
        public void TestMethodAddUserWithEmptyName()
        {
            try
            {
                var dataStore = new InMemoryDataStore();
                var user = new User { UserName = null };
                dataStore.AddOrUpdateUser(user);
            }
            catch (ArgumentNullException exception)
            {
                Assert.IsTrue(string.Equals(exception.Message, "user name can't be null", StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception exception)
            {
                Debug.WriteLine(format: "Can't add user {0}", args: exception);
                throw;
            }
        }

        [TestMethod]
        public void TestMethodUpdateUser()
        {
            try
            {
                var dataStore = new InMemoryDataStore();
                var user = new User { UserName = "Test1" };
                int id = dataStore.AddOrUpdateUser(user);
                Assert.IsTrue(id > 0);
                var loadUser = dataStore.LoadUser(id);
                loadUser.UserName = "Test2";
                int newid = dataStore.AddOrUpdateUser(loadUser);
                var updatedUser = dataStore.LoadUser(newid);
                Assert.IsTrue(updatedUser.UserId == id && string.Equals(updatedUser.UserName, loadUser.UserName), "Load user is not the same");
            }
            catch (Exception exception)
            {
                Debug.WriteLine(format: "Can't update user {0}", args: exception);
                throw;
            }
        }

        [TestMethod]
        public void TestMethodAddMessage()
        {
            try
            {
                var dataStore = new InMemoryDataStore();
                var message = new Message(1, "test", DateTime.UtcNow);
                dataStore.AddMessage(message);
                var messages = dataStore.LoadPreviousMessages(DateTime.UtcNow);
                Assert.IsTrue(messages.Count == 1);
                var loadMessage = messages.First();
                Assert.IsTrue(loadMessage.OwnerId == message.OwnerId && string.Equals(loadMessage.Content, message.Content));
            }
            catch (Exception exception)
            {
                Debug.WriteLine(format: "Can't add message {0}", args: exception);
                throw;
            }
        }
    }
}
