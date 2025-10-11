using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using PizzaShed.Commands;
using PizzaShed.Model;
using PizzaShed.Services.Data;


namespace PizzaShed.ViewModels
{
    public class CustomerViewModel : ViewModelBase
    {
        private IOrderRepository _orderRepository;
        private ICustomerRepository _customerRepository;
        private readonly int _orderId;

        public int OrderID => _orderId;

        private Customer _currentCustomer = new Customer();
        public Customer CurrentCustomer
        {
            get => _currentCustomer;
            set
            {                
                SetProperty(ref _currentCustomer, value);
                // Update the name displayed on the user form
                NameSearch = _currentCustomer.Name;                   
            }
        }

        // We need to use a nullable proxy for setting the
        // current customer from the ListView selection
        // using CurrentCustomer directly results in null references
        private Customer? _proxyCustomer;
        public Customer? ProxyCustomer
        {
            get => _proxyCustomer;
            set
            {
                SetProperty(ref _proxyCustomer, value);
                if (ProxyCustomer != null)
                {
                    CurrentCustomer = ProxyCustomer;
                }
            }
        }

        private ObservableCollection<Customer> _customerSuggestion = [];
        public ObservableCollection<Customer> CustomerSuggestion
        {
            get => _customerSuggestion;
            set
            {
                SetProperty(ref _customerSuggestion, value);                            
            }
        }

        private string _nameSearch = "";
        public string NameSearch
        {
            get =>_nameSearch;
            set
            {
                if (CurrentCustomer.ID == 0 && value.Length > 0)
                {
                    SetProperty(ref _nameSearch, value);
                    CurrentCustomer.Name = value;
                    CustomerSuggestion = _customerRepository.GetCustomerByPartialName(_nameSearch);
                }
                else
                {
                    SetProperty(ref _nameSearch, value);
                }
            }
        }

        public string _errorMessage = "";
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand ClearCommand { get; }
        public ICommand BackCommand { get; }

        public ICommand CheckoutCommand { get; }
        public CustomerViewModel(IOrderRepository orderRepository, ICustomerRepository customerRepository, int orderId)
        {
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _orderId = orderId;

            ClearCommand = new RelayGenericCommand(OnClear);
            BackCommand = new RelayGenericCommand(OnBack);
            CheckoutCommand = new RelayGenericCommand(OnCheckout);
        }

        private void OnClear()
        {
            CustomerSuggestion = [];
            CurrentCustomer = new Customer();
        }
        private void OnBack()
        {
            OnNavigateBack();
        }

        private void OnCheckout()
        {
            ErrorMessage = "";
            // Don't progress if we have errors or missing info
            if (
                CurrentCustomer.PostcodeError != ""
                || CurrentCustomer.NumberError != ""
                || CurrentCustomer.StreetAddress == ""
                || CurrentCustomer.Name == ""
                || CurrentCustomer.House == ""
                || CurrentCustomer.Postcode == ""
                || CurrentCustomer.PhoneNumber == ""
            )
                return;
            
            // Handle order for existing customer            
            if (CurrentCustomer.ID != 0)
            {
                if (!_customerRepository.UpdateCustomer(CurrentCustomer))
                {
                    ErrorMessage = "Failed to update\n customer record";
                    return;
                }                    
            } 
            else
            {
                CurrentCustomer.ID = _customerRepository.CreateNewCustomer(CurrentCustomer);
                if (CurrentCustomer.ID == 0)
                {
                    ErrorMessage = "Failed to create\n new customer";
                    return;
                }
            }

            int distance = GetDistanceInMiles();
            if (distance < 0 || distance > 4)
            {
                ErrorMessage = "Error calculating\n distance";
                return;
            }

            if (!_orderRepository.UpdateDeliveryOrder(OrderID, CurrentCustomer.ID, distance))
            {
                ErrorMessage = "Failed to update\n Order";
                return;
            }

            OnNavigate();
        }

        // Helper function for distance calculation 
        private int GetDistanceInMiles()
        {
            // We assume here that the shops postcode is TA6 5**
            // distance is calculated as the difference between
            // the 4th numeric character of the shop and customers postcode

            // We remove any whitespace from the postcode so we can get the correct character            
            string postcode = Regex.Replace(CurrentCustomer.Postcode, @"\s+", "");

            // Try and convert the digit
            if (int.TryParse(postcode[3].ToString(), out int digit))
            {
                // Get the difference
                int distance = 5 - digit;

                // If the postcode starts with 0 we still deliver at the higher rate
                if (distance == 5)
                    return distance - 1;

                // If we have a negative integer we convert it to a positive integer here
                return Math.Abs(distance);
            }
            return -1;
        }
    }
}
