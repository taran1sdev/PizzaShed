using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PizzaShed.Services.Data;
using PizzaShed.Model;

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

        // When the session is changed navigate to the view that matches the user's role
        private void OnSessionChanged(object? sender, EventArgs e)
        {
            if(_session.IsLoggedIn)
            {
                NavigateToView(_session.UserRole);
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
                default:
                    var ProductRepository = new ProductRepository();
                    var ToppingRepository = new ToppingRepository();
                    CurrentViewModel = new CashierViewModel(ProductRepository, ToppingRepository, _session);
                    break;
            }
        }
    }
}
