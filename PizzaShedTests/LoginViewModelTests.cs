using Moq;
using Moq.Protected;
using NUnit.Framework;
using PizzaShed.Model;
using PizzaShed.Services.Data;
using PizzaShed.ViewModels;
using PizzaShed.Commands;
using PizzaShed.Services.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaShedTests
{
    [TestFixture]
    public class LoginViewModelTests
    {
        private Mock<IUserRepository> _userRepository;
        private Mock<ISession> _session;
        private LoginViewModel _loginViewModel;

        [SetUp]
        public void Setup()
        {
            _userRepository = new Mock<IUserRepository>();
            _session = new Mock<ISession>();
            _loginViewModel = new LoginViewModel(_userRepository.Object, _session.Object);
        }


        // We ensure that our RelayCommand is being initialized correctly
        [Test]
        public void Constructor_Initializes_Command()
        {
            Assert.That(_loginViewModel.ButtonCommand, Is.Not.Null);
            Assert.That(_loginViewModel.ButtonCommand, Is.InstanceOf<RelayCommand<string>>());
        }

        // We ensure ButtonCommand appends digits to our Pin property when called
        [Test]
        public void ButtonCommand_AppendsDigit()
        {
            _loginViewModel.ButtonCommand.Execute("1");
            _loginViewModel.ButtonCommand.Execute("2");
            _loginViewModel.ButtonCommand.Execute("3");

            Assert.That(_loginViewModel.Pin, Is.EqualTo("123"));
        }


        // We ensure ButtonCommand removes a digit from the Pin property when
        // called with backspace
        [Test]
        public void ButtonCommand_RemovesDigit_OnBackspace()
        {
            _loginViewModel.Pin = "123";

            _loginViewModel.ButtonCommand.Execute("Backspace");
            _loginViewModel.ButtonCommand.Execute("Backspace");

            Assert.That(_loginViewModel.Pin, Is.EqualTo("1"));
        }

        // We ensure ButtonCommand does not throw an exception
        // when backspace is called on an empty Pin
        [Test]
        public void ButtonCommand_HandlesEmptyPin_OnBackspace()
        {
            _loginViewModel.Pin = "";

            _loginViewModel.ButtonCommand.Execute("Backspace");

            Assert.That(_loginViewModel.Pin, Is.EqualTo(""));
        }


        // We make sure ButtonCommand clears the Pin property when
        // called with Clear
        [Test]
        public void ButtonCommand_ClearsPin_OnClear()
        {
            _loginViewModel.Pin = "123";

            _loginViewModel.ButtonCommand.Execute("Clear");

            Assert.That(_loginViewModel.Pin, Is.EqualTo(""));
        }


        // We ensure the Pin properties setter clears any error message
        // when new input is recieved
        [Test]
        public void Pin_ClearsError_OnInput()
        {
            _loginViewModel.ErrorMessage = "Error";

            _loginViewModel.ButtonCommand.Execute("1");

            Assert.That(_loginViewModel.ErrorMessage, Is.EqualTo(""));
        }

        // We ensure the Pin property calls AttemptLogin when the Pin length reaches 4
        [Test]
        public void Pin_AttemptsLogin_OnLengthOfFour()
        {            
            string expectedPin = "1234";
            string expectedHashedPin = PasswordHasher.HashPin(expectedPin);
            User testUser = new User(1, "test", "test");

            _userRepository.Setup(u => u.GetUserByPin(expectedHashedPin))
                           .Returns(testUser);

            _loginViewModel.Pin = expectedPin;

            _userRepository.Verify(u => u.GetUserByPin(expectedHashedPin),
                                    Times.Once(), "GetUserByPin should only be called once");

            _session.Verify(s => s.Login(testUser),
                            Times.Once(), "Login should only be called once");

            Assert.That(_loginViewModel.Pin, Is.EqualTo(""), "Pin should be reset to empty value on Login");
        }

        // We ensure that the Pin is reset and and Error Message is displayed on failed login
        [Test]
        public void PinCleared_ErrorDisplayed_OnFailedLogin()
        {
            string expectedPin = "1234";
            string expectedHashedPin = PasswordHasher.HashPin(expectedPin);

            User? testUser = null;

            _userRepository.Setup(u => u.GetUserByPin(expectedHashedPin))
                           .Returns(testUser);     

            _loginViewModel.Pin = expectedPin;

            _userRepository.Verify(u => u.GetUserByPin(expectedHashedPin),
                                   Times.Once(), "GetUserByPin should only be called once");

            _session.Verify(s => s.Login(testUser),
                            Times.Never(), "Login should not be called on failed login");

            Assert.That(_loginViewModel.ErrorMessage, Is.Not.EqualTo(""));
            Assert.That(_loginViewModel.Pin, Is.EqualTo(""));
        }        
    }
}
