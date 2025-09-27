using NUnit.Framework;
using Moq;
using PizzaShed.Services.Data;
using PizzaShed.Services.Security;
using PizzaShed.ViewModels;

namespace PizzaShedTests
{
    [TestFixture]
    public class LoginViewModelTest
    {
        private Mock<IUserRepository> _mockUserRepository;
        private LoginViewModel _loginViewModel;

        [SetUp]
        public void Setup()
        {
            // Initialize a mock user repo and login view model before running tests
            _mockUserRepository = new Mock<IUserRepository>();
            //_loginViewModel = new LoginViewModel(_mockUserRepository.Object);
        }

        [Test]
        // Ensure that all values are initialized correctly
        public void Properties_Initialized()
        {
            Assert.That(_loginViewModel.Pin, Is.EqualTo(""), "Pin should be an empty string");
            Assert.That(_loginViewModel.ErrorMessage, Is.EqualTo(""), "ErrorMessage should be an empty string");
            Assert.That(_loginViewModel.ButtonCommand, Is.Not.Null, "ButtonCommand should be initialized");
        }

        [Test]
        // Check ExecuteButtonCommand appends digit to Pin 
        public void ExectuteButtonCommand_Appends_Digit_To_Pin()
        {
            _loginViewModel.Pin = "12";

            _loginViewModel.ButtonCommand.Execute("3");

            Assert.That(_loginViewModel.Pin, Is.EqualTo("123"), "Digit should be appended to Pin");
        }

        [Test]
        // Check ExecuteButtonCommand clears Pin
        public void ExecuteButtonCommand_Clears_Pin()
        {
            _loginViewModel.Pin = "12";

            _loginViewModel.ButtonCommand.Execute("Clear");

            Assert.That(_loginViewModel.Pin, Is.EqualTo(""), "Pin should be an empty string");
        }

        [Test]
        // Check ExecuteButtonCommand removes last digit
        public void ExecuteButtonCommand_Backspaces_Pin()
        {
            _loginViewModel.Pin = "123";

            _loginViewModel.ButtonCommand.Execute("Backspace");

            Assert.That(_loginViewModel.Pin, Is.EqualTo("12"), "Pin should have one less character");
        }

        [Test]
        // Check AttemptLogin gets called when pin length is 4 
        public void Pin_AttemptsLogin_OnFourDigits_Success()
        {
            const string pinToTest = "0000";
            string expectedPinHash = PasswordHasher.HashPin(pinToTest);

            // Set the mock to return true when called with the expected pin hash
            _mockUserRepository.Setup(r => r.GetUserByPin(expectedPinHash)).Returns(true);

            _loginViewModel.Pin = pinToTest;

            // Make sure the method is only being called once
            _mockUserRepository.Verify(r => r.GetUserByPin(expectedPinHash), Times.Once);

            // Make sure the pin is not removed 
            Assert.That(_loginViewModel.Pin, Is.EqualTo(pinToTest), "Pin should remain when successful");
            // Make sure the ErrorMessage is empty
            Assert.That(_loginViewModel.ErrorMessage, Is.EqualTo(""), "Error message should be empty on success");
        }

        [Test]
        public void Pin_AttemptsLogin_OnFourDigits_Failure()
        {
            const string pinToTest = "9999";
            string expectedPinHash = PasswordHasher.HashPin(pinToTest);

            // Set the mock to return false when called with the expected pin hash
            _mockUserRepository.Setup(r => r.GetUserByPin(expectedPinHash)).Returns(false);

            _loginViewModel.Pin = pinToTest;

            // Make sure the method is only being called once
            _mockUserRepository.Verify(r => r.GetUserByPin(expectedPinHash), Times.Once);

            // Make sure the pin is removed
            Assert.That(_loginViewModel.Pin, Is.EqualTo(""), "Pin should be removed on failure");
            // Make sure the error message is displayed
            Assert.That(_loginViewModel.ErrorMessage, Is.Not.EqualTo(""), "Error message should be displayed on failure");
        }

        [Test]
        // Make sure the error message is removed once the user enters a new digit
        public void ErrorMessage_Clears_OnUserInput()
        {            
            _loginViewModel.ErrorMessage = "Login failed...";

            // Simulate user input
            _loginViewModel.ButtonCommand.Execute("1");

            // Check the error message is cleared
            Assert.That(_loginViewModel.ErrorMessage, Is.EqualTo(""), "Error message should be removed on user input");
        }       
    }
}