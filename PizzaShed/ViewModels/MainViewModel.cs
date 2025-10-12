using PizzaShed.Model;
using PizzaShed.Services.Data;
using PizzaShed.Services.Logging;
using PizzaShed.Views.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Provider;

namespace PizzaShed.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ISession _session;
        private readonly IProductRepository<Product> _productRepository;
        private readonly IProductRepository<Topping> _toppingRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerRepository _customerRepository;
        
        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);                
            
        }

        private ViewModelBase _paymentViewModel;
        public ViewModelBase PaymentViewModel
        {
            get => _paymentViewModel;
            set => SetProperty(ref _paymentViewModel, value);
        }

        private PaymentWindow _paymentWindow;

        public MainViewModel(
            ISession session, 
            IUserRepository userRepository, 
            IProductRepository<Product> productRepository,
            IProductRepository<Topping> toppingRepository,
            IOrderRepository orderRepository,
            ICustomerRepository customerRepository
            )
        {
            _session = session;
            _userRepository = userRepository;
            _productRepository = productRepository;
            _toppingRepository = toppingRepository;
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _currentViewModel = this;

            _paymentWindow = new PaymentWindow();
            _paymentWindow.DataContext = this;
            
            _paymentViewModel = new ViewModelBase();            
            
            _session.SessionChanged += OnSessionChanged;

            OnSessionChanged(this, EventArgs.Empty);
        }        

        // When the session is changed navigate to the view that matches the user's role
        private void OnSessionChanged(object? sender, EventArgs e)
        {
            try
            {
                switch (_session.UserRole.ToLower())
                {
                    case "cashier" or "manager":                        
                        CurrentViewModel = new CashierViewModel(_productRepository, _toppingRepository, _orderRepository, _session, []);
                        CurrentViewModel.Navigate += OnCheckout;
                        CurrentViewModel.NavigateBack += OnCollection;
                        break;
                    case "pizzaiolo" or "grill cook":
                        CurrentViewModel = new OrderViewModel(_session, _orderRepository, _customerRepository);                        
                        break;
                    case "driver": // We handle driver seperately as they may need to navigate to checkout view
                        CurrentViewModel = new OrderViewModel(_session, _orderRepository, _customerRepository);
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

                // We need to convert to the class type to access the OrderID Property
                if (CurrentViewModel is CashierViewModel cashierView)
                {
                    CurrentViewModel.NavigateBack -= OnCollection;
                    // If we have a delivery order get the customer info before checkout
                    if (cashierView.IsDelivery)
                    {
                        CurrentViewModel = new CustomerViewModel(_orderRepository, _customerRepository, cashierView.OrderID);
                        // We still want to navigate to checkout once we get customer info
                        CurrentViewModel.Navigate += OnCheckout;
                        CurrentViewModel.NavigateBack += OnCheckoutBack;
                    } 
                    else
                    {
                        CurrentViewModel = new CheckoutViewModel(_orderRepository, _session, cashierView.OrderID);
                        CurrentViewModel.Navigate += ViewPayment;
                        CurrentViewModel.NavigateBack += OnCheckoutBack;
                    }                        
                } 
                else if (CurrentViewModel is CustomerViewModel customerView)
                {
                    CurrentViewModel.Navigate -= OnCheckout;
                    CurrentViewModel = new CheckoutViewModel(_orderRepository, _session, customerView.OrderID);
                    CurrentViewModel.Navigate += ViewPayment;
                    CurrentViewModel.NavigateBack += OnCheckoutBack;                    
                } 
                else if (CurrentViewModel is OrderViewModel orderView)
                {
                    if (_session.UserRole == "Cashier")
                        CurrentViewModel.NavigateBack -= OnCollection;

                    CurrentViewModel = new CheckoutViewModel(_orderRepository, _session, orderView.OrderID);
                    CurrentViewModel.Navigate += ViewPayment;
                    CurrentViewModel.NavigateBack += OnCollection;
                }                                                
            } 
            catch (Exception ex)
            {
                EventLogger.LogError("Error navigating to checkout " + ex.Message);
            }
        }

        private void OnCollection(object? sender, EventArgs e)
        {
            if (CurrentViewModel is CashierViewModel cashierViewModel)
            {
                CurrentViewModel.Navigate -= OnCheckout;
                CurrentViewModel.NavigateBack -= OnCollection;
                CurrentViewModel = new OrderViewModel(_session, _orderRepository, _customerRepository);
                CurrentViewModel.Navigate += OnCheckout;
                CurrentViewModel.NavigateBack += OnCollectionBack;
            }
            else if (CurrentViewModel is CheckoutViewModel checkoutViewModel)
            {
                CurrentViewModel.Navigate -= ViewPayment;
                CurrentViewModel.NavigateBack -= OnCollection;
                CurrentViewModel = new OrderViewModel(_session, _orderRepository, _customerRepository);
                CurrentViewModel.Navigate += OnCheckout;
                if (_session.UserRole == "Cashier")
                    CurrentViewModel.NavigateBack += OnCollectionBack;
            }
        }

        private void OnCollectionBack(object? sender, EventArgs e)
        {
            if (CurrentViewModel is OrderViewModel orderViewModel)
            {
                CurrentViewModel.NavigateBack -= OnCollectionBack;
                CurrentViewModel = new CashierViewModel(_productRepository, _toppingRepository, _orderRepository, _session, []);
                CurrentViewModel.Navigate += OnCheckout;
                CurrentViewModel.NavigateBack += OnCollection;
            } 
        }

        // This function will handle navigation from Checkout / Customer view back to cashier
        private void OnCheckoutBack(object? sender, EventArgs e)
        {
            try
            {
                CurrentViewModel.NavigateBack -= OnCheckoutBack;
                if (CurrentViewModel is CheckoutViewModel checkoutView)
                {                    
                    CurrentViewModel = new CashierViewModel(_productRepository, _toppingRepository, _orderRepository, _session, checkoutView.CurrentOrder);
                    CurrentViewModel.Navigate += OnCheckout;
                }
                else if (CurrentViewModel is CustomerViewModel customerView)
                {
                    CurrentViewModel.Navigate -= OnCheckout;
                    
                    // Since we aren't passing the products to the Customer view model we retrieve / delete them here
                    Order? currentOrder = _orderRepository.GetOrderByOrderNumber(customerView.OrderID);                    
                    ObservableCollection<Product> orderProducts = currentOrder == null ? [] : currentOrder.OrderProducts;

                    _orderRepository.DeleteOrder(customerView.OrderID);

                    CurrentViewModel = new CashierViewModel(_productRepository, _toppingRepository, _orderRepository, _session, orderProducts);
                    CurrentViewModel.Navigate += OnCheckout;
                }
                
            }
            catch (Exception ex)
            {
                EventLogger.LogError("Error navigating from checkout to cashier view: " + ex.Message);
            }
        }

        // This function will display our payment popup window
        private void ViewPayment(object? sender, EventArgs e)
        {
            try
            {
                if (CurrentViewModel is CheckoutViewModel checkoutViewModel)
                {
                    // Handle cash on delivery / collection
                    if (checkoutViewModel.IsPaid)
                    {                        
                        checkoutViewModel.AcceptOrder = false;
                        CurrentViewModel.Navigate -= ViewPayment;
                        CurrentViewModel.Navigate += OnCompleteOrder;
                        return;
                    }

                    if (checkoutViewModel.IsCollection && !checkoutViewModel.IsPhone)
                    {                        
                        _paymentWindow.Show();                        
                        PaymentViewModel = new PaymentPresentViewModel(checkoutViewModel);
                        checkoutViewModel.Navigate -= ViewPayment;
                        checkoutViewModel.Navigate += OnPayment;
                    }
                    else
                    {                        
                        _paymentWindow.Show();                     
                        PaymentViewModel = new PaymentNotPresentViewModel(checkoutViewModel);
                        checkoutViewModel.Navigate -= ViewPayment;
                        checkoutViewModel.Navigate += OnPayment;
                    }
                }
            }
            catch (Exception ex)
            {
                EventLogger.LogError("Error occured displaying payment window " + ex.Message);
            }            
        }

        private void OnPayment(object? sender, EventArgs e)
        {            
            try
            {
                _paymentWindow.Hide();


                if (CurrentViewModel is CheckoutViewModel checkoutViewModel)
                {
                    if (checkoutViewModel.IsPaid)
                    {
                        checkoutViewModel.AcceptOrder = false;
                        CurrentViewModel.Navigate -= OnPayment;
                        CurrentViewModel.NavigateBack -= OnCheckoutBack;

                        CurrentViewModel.Navigate += OnCompleteOrder;
                        CurrentViewModel.NavigateBack += OnCompleteOrder;                        
                    }
                    else
                    {
                        CurrentViewModel.Navigate -= OnPayment;
                        CurrentViewModel.Navigate += ViewPayment;
                    }


                }
            }    
            catch (Exception ex)
            {
                EventLogger.LogError("Error occured during payment navigation " + ex.Message);
            }
        }        

        private void OnCompleteOrder(object? sender, EventArgs e)
        {            
            CurrentViewModel.Navigate -= OnCompleteOrder;
            CurrentViewModel.NavigateBack -= OnCheckoutBack;
            
            
            CurrentViewModel = new CashierViewModel(_productRepository, _toppingRepository, _orderRepository, _session, []);
            CurrentViewModel.Navigate += OnCheckout;
            CurrentViewModel.NavigateBack += OnCollection;
        }
    }
}
