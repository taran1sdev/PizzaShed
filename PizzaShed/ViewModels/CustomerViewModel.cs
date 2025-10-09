using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PizzaShed.Model;
using PizzaShed.Services.Data;


namespace PizzaShed.ViewModels
{
    public class CustomerViewModel : ViewModelBase
    {
        private IOrderRepository _orderRepository;
        private int _orderId;

        public int OrderID => _orderId;

        private Customer? _currentCustomer;
        public Customer? CurrentCustomer
        {
            get => _currentCustomer;
            set => SetProperty(ref _currentCustomer, value);
        }

        public CustomerViewModel(IOrderRepository orderRepository, int orderId)
        {
            _orderRepository = orderRepository;
            _orderId = orderId;
        }


    }
}
