using PizzaShed.Services.Data;
using PizzaShed.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PizzaShed.Commands;

namespace PizzaShed.ViewModels
{
    public class OrderViewModel : ViewModelBase
    {
        private readonly ISession _session;
        private IOrderRepository _orderRepository;
        private ICustomerRepository _customerRepository;

        // This property is only for navigating to checkout 
        public int OrderID { get; set; } = 0;

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

        // We keep a single collection for all orders 
        private ObservableCollection<Order> allOrders = [];

        // and create collections that are filtered by status
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

        public ICommand CompleteOrderCommand { get; }

        public ICommand LogoutCommand { get; }
        public ICommand BackCommand { get; }

        public OrderViewModel(ISession session, IOrderRepository orderRepository, ICustomerRepository customerRepository)
        {
            _session = session;
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;

            UpdateView();

            if (_session.UserRole == "Pizzaiolo" || _session.UserRole == "Grill Cook")
            {
                CompleteOrderCommand = new RelayCommand<int>(CompleteKitchenOrder);
            }
            else
            {
                CompleteOrderCommand = new RelayCommand<int>(CompleteOrder);
            }                                        

            LogoutCommand = new RelayGenericCommand(Logout);
            BackCommand = new RelayGenericCommand(Back);
        }

        private void UpdateView()
        {            
            switch (_session.UserRole)
            {
                case "Cashier":
                    allOrders = _orderRepository.GetCollectionOrders();
                    NewOrders = [.. allOrders.ToList().Where(o => o.OrderStatus != "Order Ready")];
                    ReadyOrders = [.. allOrders.ToList().Where(o => o.OrderStatus == "Order Ready")];
                    break;
                case "Pizzaiolo":
                    allOrders = _orderRepository.GetKitchenOrders(true);
                    NewOrders = [.. allOrders.ToList().Where(o => o.OrderStatus == "New")];
                    ReadyOrders = [.. allOrders.ToList().Where(o => o.OrderStatus == "Preparing")];
                    break;
                case "Grill Cook":
                    allOrders = _orderRepository.GetKitchenOrders(false);
                    NewOrders = [.. allOrders.ToList().Where(o => o.OrderStatus == "New")];
                    ReadyOrders = [.. allOrders.ToList().Where(o => o.OrderStatus == "Preparing")];
                    break;
                case "Driver":
                    allOrders = _orderRepository.GetDeliveryOrders();
                    // We need to retrieve the customer info for delivery orders
                    foreach (Order o in allOrders)
                    {
                        if (o.CustomerID != null)
                            o.Customer = _customerRepository.GetCustomerByID((int)o.CustomerID);
                    }
                    NewOrders = [.. allOrders.ToList().Where(o => o.OrderStatus == "Order Ready")];
                    ReadyOrders = [.. allOrders.ToList().Where(o => o.OrderStatus == "Out For Delivery")];
                    break;
            }
        }

        private void CompleteOrder(int orderID)
        {
            Order orderToComplete = allOrders.ToList().First(o => o.ID == orderID);
            
            if (orderToComplete.OrderStatus == "Order Ready")
            {
                _orderRepository.DeliverOrder(orderID);
                UpdateView();
            } 
            else if (orderToComplete.Paid)
            {
                _orderRepository.CompleteOrder(orderToComplete.ID);
                UpdateView();
            } 
            else // Order needs to be paid
            {
                OrderID = orderID;
                OnNavigate();
            }
        }

        private void CompleteKitchenOrder(int orderID)
        {
            // Because we have 2 kitchen stations we update the view in case the ready
            // status of the other station has changed            
            Order orderToComplete = allOrders.ToList().First(o => o.ID == orderID);

            if (orderToComplete.OrderStatus == "New")
            {
                _orderRepository.PrepareOrder(orderID);                    
            } 
            else
            {
                UpdateView();

                if (_session.UserRole == "Pizzaiolo")
                {
                    // Pizza's and grills ready then complete the order
                    if (orderToComplete.GrillReady)
                    {
                        _orderRepository.OrderReady(orderID);
                    }
                    else
                    {
                        // Just Pizza ready then update the database 
                        _orderRepository.CompleteOrderStation(orderID, true);
                    }
                }
                else if (_session.UserRole == "Grill Cook")
                {
                    if (orderToComplete.PizzaReady)
                    {
                        _orderRepository.OrderReady(orderID);
                    }
                    else
                    {
                        _orderRepository.CompleteOrderStation(orderID, false);
                    }
                }
            }            

            UpdateView();
        }

        private void Back() => OnNavigateBack();
        

        private void Logout() => _session.Logout();        
    }
}
