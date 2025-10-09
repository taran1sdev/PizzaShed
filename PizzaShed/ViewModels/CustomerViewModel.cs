using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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

        public ICommand BackCommand { get; }
        public CustomerViewModel(IOrderRepository orderRepository, ICustomerRepository customerRepository, int orderId)
        {
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _orderId = orderId;

            BackCommand = new RelayGenericCommand(OnBack);
        }

        public void OnBack()
        {
            OnNavigateBack();
        }
    }
}
