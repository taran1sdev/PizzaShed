using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PizzaShed.Services.Data;

namespace PizzaShed.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ISession _session;
        private ViewModelBase _currentViewModel;

        public MainViewModel(IUserRepository userRepository, ISession session)
        {
            _userRepository = userRepository;
            _session = session;

            _session.SessionChanged += OnSessionChanged;

            OnSessionChanged(this, EventArgs.Empty);
        }

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged();
            }
        }

        private void OnSessionChanged(object? sender, EventArgs e)
        {
            if(_session.IsLoggedIn)
            {
                // navigate to page
            }
            else
            {
                CurrentViewModel = new LoginViewModel(this, _userRepository, _session);
            }
        }
    
        public void NavigateToView(string role)
        {
            switch (role.ToLower())
            {                
                case "pizzaiolo":
                    // Change
                    break;
                case "grill cook":
                    // change
                    break;
                case "driver":
                    // change
                    break;
                case "manager":
                    // change
                    break;
                default:
                    // cashier
                    break;
            }
        }
    }
}
