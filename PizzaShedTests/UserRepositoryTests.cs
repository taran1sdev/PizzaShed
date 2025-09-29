using NUnit.Framework;
using Moq;
using Microsoft.Data.SqlClient;
using PizzaShed.Services.Data;
using PizzaShed.Services.Security;
using PizzaShed.Model;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace PizzaShedTests
{
    // Before we run these tests we have to remove the sealed keyword from DatabaseManager and Session
    // We need to also remove the readonly keyword from the instance field in both classes
    
    // We need to make a few changes to source for these tests to run
    // 1. Remove the sealed keyword from the DatabaseManager and Session class definitions
    // 2. Remove the readonly keyword from the instance field in the DatabaseManager and Session classes
    // 3. Add the virtual keyword to the DatabaseManager.ExecuteQuery and Session.Login functions
    
    [TestFixture]
    public class UserRepositoryTests
    {
        private UserRepository _userRepository;
        private Mock<DatabaseManager> _mockDbManager;
        private Mock<Session> _mockSession;

        private FieldInfo _dbInstanceField;
        private FieldInfo _sessionInstanceField;

        [SetUp]
        public void SetUp()
        {            
            _mockDbManager = new Mock<DatabaseManager>();
            _mockSession = new Mock<Session>();

            // We overwrite the instances of the classes used by UserRepository to mock instances for testing
            _dbInstanceField = typeof(DatabaseManager).GetField("instance", BindingFlags.Static | BindingFlags.NonPublic);
            _dbInstanceField.SetValue(null, _mockDbManager.Object);


            _sessionInstanceField = typeof(Session).GetField("instance", BindingFlags.Static | BindingFlags.NonPublic);
            _sessionInstanceField.SetValue(null, _mockSession.Object);

            _userRepository = new UserRepository();   
        
        
        }

        [TearDown]
        // Reset the singleton instances between tests
        public void TearDown()
        {
            _dbInstanceField.SetValue(null, null);
            _sessionInstanceField.SetValue(null, null);
        }

        [Test]
        // Check GetUserByPin returns true on valid input
        public void GetUserByPin_Returns_True_On_Success()
        {
            // We always return a valid user object for testing 
            _mockDbManager.Setup(r => r.ExecuteQuery(
                It.IsAny<Func<SqlConnection, User>>()))
                .Returns(new User(1, "user", "test")
                );

            // Call the function with any input
            User result = _userRepository.GetUserByPin("1234");

            Assert.IsTrue(result == new User(1, "user", "test"), "GetUserByPin should return User when ExecuteQuery returns a User Object");
        }

        [Test]
        // Check GetUserByPin returns false on invalid input
        public void GetUserByPin_Returns_False_On_Failure()
        {
            User? nullReturn = null;

            // We always return null for testing
            _mockDbManager.Setup(r => r.ExecuteQuery(
                It.IsAny<Func<SqlConnection, User?>>()))
                .Returns(nullReturn);

            User? result = _userRepository.GetUserByPin("1234");

            Assert.IsFalse(result == null, "GetUserByPin should return false when ExecuteQuery returns null");
        }

        [Test]
        // Check GetUserByPin returns false on Exception
        public void GetUserByPin_Returns_False_On_Exception()
        {
            _mockDbManager.Setup(r => r.ExecuteQuery(
                It.IsAny<Func<SqlConnection, User>>()))
                .Throws<Exception>();

            User? result = _userRepository.GetUserByPin("1234");

            Assert.IsTrue(result == null, "GetUserByPin should return null when ExecuteQuery throws an exception");
        }           
    }
}
