using PizzaShed.Model;
using PizzaShed.Services.Data;
using PizzaShed.Services.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            _currentViewModel = this;

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

        // When the session is changed navigate to the view that matches the user's role
        private void OnSessionChanged(object? sender, EventArgs e)
        {
            try
            {
                switch (_session.UserRole.ToLower())
                {
                    case "cashier" or "manager":
                        var ProductRepository = new ProductRepository();
                        var ToppingRepository = new ToppingRepository();
                        CurrentViewModel = new CashierViewModel(ProductRepository, ToppingRepository, _session, []);
                        break;
                    default:
                        CurrentViewModel = new LoginViewModel(_userRepository, _session);
                        break;
                }                              
            }
            catch (Exception ex)
            {
                EventLogger.LogError("Error navigating to view " + ex.Message);
            }
        }    
    }
}
