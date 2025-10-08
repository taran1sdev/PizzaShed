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
using System.Windows.Automation.Provider;

namespace PizzaShed.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ISession _session;
        private IProductRepository<Product> _productRepository;
        private IProductRepository<Topping> _toppingRepository;
        private IOrderRepository _orderRepository;
        private ViewModelBase _currentViewModel;

        public MainViewModel(IUserRepository userRepository, ISession session)
        {
            _userRepository = userRepository;
            _session = session;
            _productRepository = new ProductRepository(DatabaseManager.Instance);
            _toppingRepository = new ToppingRepository(DatabaseManager.Instance);
            _orderRepository = new OrderRepository(DatabaseManager.Instance);
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
                        
                        CurrentViewModel = new CashierViewModel(_productRepository, _toppingRepository, _orderRepository, _session, null);
                        CurrentViewModel.Navigate += OnCheckout;
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

        // Function to navigate to the checkout view once an order has been created successfully
        private void OnCheckout(object? sender, EventArgs e)
        {
            try
            {
                // Unsubscribe the event to avoid memory leaks
                CurrentViewModel.Navigate -= OnCheckout;
                CurrentViewModel = new CheckoutViewModel(_orderRepository, _session);                                
            } 
            catch (Exception ex)
            {
                EventLogger.LogError("Error navigating to checkout " + ex.Message);
            }
        }
    }
}
