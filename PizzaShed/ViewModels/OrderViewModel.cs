using PizzaShed.Services.Data;
using PizzaShed.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaShed.ViewModels
{
    public class OrderViewModel : ViewModelBase
    {
        private readonly ISession _session;
        private IOrderRepository _orderRepository;

        public bool IsCashier => _session.UserRole == "Cashier";        
        public bool IsCook => _session.UserRole == "Grill Cook" || _session.UserRole == "Pizzaiolo";
        public bool IsDriver => _session.UserRole == "Driver";

        public string NewLabel
        {
            get
            {
                switch (_session.UserRole)
                {
                    case "Cashier":
                        return "Orders In Progress";
                    case "Driver":
                        return "Ready For Delivery";
                    default:
                        return "New Orders";
                }
            }
        }

        public string ReadyLabel
        {
            get
            {
                switch (_session.UserRole)
                {
                    case "Cashier":
                        return "Ready For Collection";
                    case "Driver":
                        return "Out for Delivery";
                    default:
                        return "Preparing";
                }
            }
        }

        private ObservableCollection<Order> _newOrders = [];
        public ObservableCollection<Order> NewOrders
        {
            get => _newOrders;
            set => SetProperty(ref _newOrders, value);
        }

        private ObservableCollection<Order> _readyOrders = [];
        public ObservableCollection<Order> ReadyOrders
        {
            get => _readyOrders;
            set => SetProperty(ref _readyOrders, value);
        }

        public OrderViewModel(ISession session, IOrderRepository orderRepository)
        {
            _session = session;
            _orderRepository = orderRepository;

            
            Order? order1 = _orderRepository.GetOrderByOrderNumber(1);
            Order? order2 = _orderRepository.GetOrderByOrderNumber(2);
            Order? order3 = _orderRepository.GetOrderByOrderNumber(3);
            ObservableCollection<Order> orders = [];
            if (order1 != null && order2 != null && order3 != null)
            {
                orders.Add(order1);
                orders.Add(order2);
                orders.Add(order3);
            }

            Order order = new Order {
                OrderStatus = "new",
                ID = 1,
                OrderProducts = []
            };

            orders.Add(order);  
            NewOrders = orders;
            OnPropertyChanged(nameof(NewOrders));
        }
    }
}
