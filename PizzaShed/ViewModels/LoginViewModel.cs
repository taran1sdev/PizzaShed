using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PizzaShed.Commands;
using PizzaShed.Services.Data;
using PizzaShed.Services.Security;
using PizzaShed.Model;

namespace PizzaShed.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IUserRepository _userRepository;
        private readonly ISession _session;
        private string _pin = "";
        private string _errorMessage = "";

        public ICommand ButtonCommand { get; }

        public LoginViewModel(MainViewModel mainViewModel, IUserRepository userRepository, ISession session)
        {
            _mainViewModel = mainViewModel;
            _userRepository = userRepository;
            _session = session;

            // Create the ButtonCommand property, initializes a RelayCommand object
            // when clicked the ExecuteButtonCommand function will be called with any
            // command parameters defined in the view
            ButtonCommand = new RelayCommand<string>(ExecuteButtonCommand);
        }

        // Holds the current pin entered by the user - this property is bound to the 
        // BoundPassword property we created in PasswordBoxHelper
        public string Pin {
            get => _pin; 
            set
            {
                if (_pin != value)
                {
                    // Update the view if the value in the password box has changed
                    _pin = value;
                    OnPropertyChanged();
                }

                // Try and login if we reach 4 characters 
                if (_pin.Length.Equals(4))
                {
                    AttemptLogin();
                }

                // Remove the error message when the user starts typing a new pin
                if (!string.IsNullOrEmpty(ErrorMessage) && _pin.Length > 0)
                {
                    ErrorMessage = "";
                }
            } 
        }
        
        // Holds the current error message being displayed to the user
        public string ErrorMessage 
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage != value)
                {
                    // Update the view if the value has changed
                    _errorMessage = value;
                    OnPropertyChanged();
                }
            } 
        }

        // Update ViewModel on User input
        private void ExecuteButtonCommand(object parameter) 
        {
            if (parameter is string buttonValue)
            {
                switch (buttonValue)
                {
                    case "Backspace":
                        if (Pin.Length > 0)
                        {
                            Pin = Pin[..^1];
                        }                        
                        break;
                    case "Clear":
                        Pin = "";
                        break;
                    default:
                        Pin += buttonValue;
                        break;
                }
            }
        }        

        // Check if the hashed pin matches a database entry
        private void AttemptLogin()
        {
            User? user = _userRepository.GetUserByPin(PasswordHasher.HashPin(Pin));
            if (user != null)
            {
                _session.Login(user);
            }            
            ErrorMessage = "Login Failed...";
            Pin = "";
        }
    }
}
