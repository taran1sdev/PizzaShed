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
        private int _orderId;

        public int OrderID => _orderId;

        private Customer? _currentCustomer;
        public Customer? CurrentCustomer
        {
            get => _currentCustomer;
            set => SetProperty(ref _currentCustomer, value);
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

        private string _customerName = "";
        public string CustomerName
        {
            get => _customerName;
            set
            {
                SetProperty(ref _customerName, value);
                CustomerSuggestion = _customerRepository.GetCustomerByPartialName(CustomerName);
            }
        }

        private string _numberError;
        public string NumberError
        {
            get => _numberError;
            set => SetProperty(ref _numberError, value);
        }

        // Regex to ensure a valid number is input
        private Regex numberRegex = new Regex(@"^\(?0( *\d\)?){10}$");
        
        private string _customerNumber;
        public string CustomerNumber
        {
            get
            {
                if (CurrentCustomer != null)
                    return CurrentCustomer.PhoneNumber;
                return _customerNumber;
            }
            set
            {
                if (numberRegex.IsMatch(value))
                {
                    SetProperty(ref _customerNumber, value);
                } else
                {
                    NumberError = "Invalid Number";
                }                    

            }
        }

        // Regex for checking postcode in area
        private Regex postcodeRangeRegex = new Regex(@"^\(?TA6( *\d\)?){1}( *\p{Lu}\)?){2}$");
        // Regex for checking valid postcode
        private Regex postcodeValidRegex = new Regex(@"^\(?( *\p{Lu}\)?){2}( *\d\)?){1}( *\p{Lu}\)?){2}$");
        public ICommand ClearCommand { get; }
        public ICommand BackCommand { get; }
        public CustomerViewModel(IOrderRepository orderRepository, ICustomerRepository customerRepository, int orderId)
        {
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _orderId = orderId;

            ClearCommand = new RelayGenericCommand(OnClear);
            BackCommand = new RelayGenericCommand(OnBack);
        }

        private void OnClear()
        {
            CurrentCustomer = null;
        }
        private void OnBack()
        {
            OnNavigateBack();
        }
    }
}
