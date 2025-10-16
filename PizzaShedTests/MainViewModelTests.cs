using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Moq;
using Moq.Protected;
using NUnit;
using PizzaShed;
using PizzaShed.Model;
using PizzaShed.Services.Data;
using PizzaShed.ViewModels;
using PizzaShed.Views.Windows;

namespace PizzaShedTests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class MainViewModelTests
    {
        private Mock<IUserRepository> _userRepository = null!;
        private Mock<IProductRepository<Product>> _productRepository = null!;
        private Mock<IProductRepository<Topping>> _toppingRepository = null!;
        private Mock<IOrderRepository> _orderRepository = null!;
        private Mock<ICustomerRepository> _customerRepository = null!;
        private Mock<ISession> _session = null!;
        private Mock<PaymentWindow> _paymentWindow = null!;
        private MainViewModel _mainViewModel = null!;
        private string _currentRole = string.Empty;

        // We create some test data
        private static Product _testProduct = new Product
        {
            ID = 1,
            Name = "Test",
            Price = (decimal)15.45,
            Category = "Pizza",
            SizeName = "Small",
        };
        private static ObservableCollection<Product> _testProducts = new() { _testProduct };
        private static Order _testOrder = new Order
        {
            ID = 1,
            OrderStatus = "New",
            OrderProducts = _testProducts,
        };

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            App app = (App)Application.Current;

            if (app == null)
            {
                app = new App();
                app.InitializeComponent();
            }
        }

        [SetUp]
        public void SetUp()
        {
            _userRepository = new Mock<IUserRepository>();
            _productRepository = new Mock<IProductRepository<Product>>();
            _toppingRepository = new Mock<IProductRepository<Topping>>();
            _orderRepository = new Mock<IOrderRepository>();
            _customerRepository = new Mock<ICustomerRepository>();

            _session = new Mock<ISession>();

            _session.SetupGet(s => s.UserRole).Returns(() => _currentRole);

            _productRepository.Setup(p => p.GetMealDeals()).Returns([]);
            _productRepository
                .Setup(p => p.GetProductsByCategory("Pizza", "Small"))
                .Returns(_testProducts.ToList());
            _toppingRepository.Setup(p => p.GetProductsByCategory("Pizza", "Small")).Returns([]);
            _orderRepository.Setup(o => o.GetCollectionOrders()).Returns([]);
            _orderRepository.Setup(o => o.GetKitchenOrders(It.IsAny<bool>())).Returns([]);
            _orderRepository.Setup(o => o.GetDeliveryOrders()).Returns([]);

            _paymentWindow = new Mock<PaymentWindow>();

            _mainViewModel = new MainViewModel(
                _session.Object,
                _userRepository.Object,
                _productRepository.Object,
                _toppingRepository.Object,
                _orderRepository.Object,
                _customerRepository.Object
            );
        }

        private void SimulateLogin(User user)
        {
            _session.SetupGet(s => s.UserRole).Returns(() => user.Role);
            _session.SetupGet(s => s.CurrentUser).Returns(user);

            _session.Raise(s => s.SessionChanged += null, EventArgs.Empty);
        }

        // Ensure that the CurrentViewModel property is being initialized correctly
        // and redirecting the user to the LoginViewModel
        [Test]
        public void CurrentViewModel_Initialized_ToSelf()
        {
            Assert.That(_mainViewModel.CurrentViewModel, Is.InstanceOf<LoginViewModel>());
        }

        [Test]
        // Ensure that CurrentViewModel is set to LoginViewModel if user is not logged in
        public async Task CurrentViewModel_EqualTo_LoginViewModel_OnNoUserSession()
        {
            SimulateLogin(new User(1, "Test", ""));

            await Task.Delay(100);

            Assert.That(_mainViewModel.CurrentViewModel, Is.InstanceOf<LoginViewModel>());
        }

        // Ensure managers / cashiers are redirected to CashierViewModel
        [TestCase("Manager")]
        [TestCase("Cashier")]
        public void OnSessionChanged_NavigatesTo_CashierViewModel(string role)
        {
            SimulateLogin(new User(1, "Test", role));

            Assert.That(_mainViewModel.CurrentViewModel, Is.InstanceOf<CashierViewModel>());
        }

        [TestCase("Pizzaiolo")]
        [TestCase("Grill Cook")]
        [TestCase("Driver")]
        public void OnSessionChanged_NavigatesTo_OrderViewModel(string role)
        {
            SimulateLogin(new User(1, "Test", role));

            Assert.That(_mainViewModel.CurrentViewModel, Is.InstanceOf<OrderViewModel>());
        }

        // We ensure that Delivery orders navigate to customer view and pass in the correct order id
        [Test]
        public void OnCheckout_Delivery_NavigatesTo_CustomerViewModel()
        {
            SimulateLogin(new User(1, "Test", "Cashier"));

            CashierViewModel _cashierViewModel = (CashierViewModel)_mainViewModel.CurrentViewModel;

            // We add a test product to satisfy the condition that triggers navigation

            _orderRepository.Setup(o => o.CreateOrder(It.IsAny<Order>())).Returns(1);

            _cashierViewModel.CurrentOrderItems = _testProducts;

            _cashierViewModel.IsDeliveryCommand.Execute(null);

            _cashierViewModel.CheckoutCommand.Execute(null);

            Assert.That(
                _mainViewModel.CurrentViewModel,
                Is.InstanceOf<CustomerViewModel>(),
                "Delivery Orders should redirect to CustomerViewModel"
            );

            var _customerViewModel = (CustomerViewModel)_mainViewModel.CurrentViewModel;
            Assert.That(
                _customerViewModel.OrderID,
                Is.EqualTo(1),
                "CustomerViewModel should recieve the returned orderID"
            );
        }

        // Ensure that the current order is deleted and it's items added to CurrentOrderItems when navigating back from customer view
        [Test]
        public void OnCheckoutBack_FromCustomer_DeletesCurrentOrder()
        {
            SimulateLogin(new User(1, "Test", "Cashier"));

            CashierViewModel _cashierViewModel = (CashierViewModel)_mainViewModel.CurrentViewModel;

            _orderRepository.Setup(o => o.CreateOrder(It.IsAny<Order>())).Returns(1);

            _orderRepository.Setup(o => o.GetOrderByOrderNumber(1)).Returns(_testOrder);

            _orderRepository.Setup(o => o.DeleteOrder(1)).Returns(true);

            _cashierViewModel.CurrentOrderItems = _testProducts;
            _cashierViewModel.IsDeliveryCommand.Execute(null);

            _cashierViewModel.CheckoutCommand.Execute(null);

            CustomerViewModel _customerViewModel = (CustomerViewModel)
                _mainViewModel.CurrentViewModel;

            _customerViewModel.BackCommand.Execute(null);

            _orderRepository.Verify(
                o => o.DeleteOrder(1),
                Times.Once(),
                "DeleteOrder should be called once"
            );

            Assert.That(
                _mainViewModel.CurrentViewModel,
                Is.InstanceOf<CashierViewModel>(),
                "OnCheckoutBack should redirect to CashierViewModel"
            );

            CashierViewModel _newCashierView = (CashierViewModel)_mainViewModel.CurrentViewModel;

            Assert.That(
                _newCashierView.CurrentOrderItems,
                Is.EquivalentTo(_testProducts),
                "CashierView OrderProducts should contain products from the deleted order"
            );
        }

        // Ensure OnCheckoutBack from CheckoutViewModel redirects to CashierView
        [Test]
        public void OnCheckoutBack_FromCheckout_RedirectsToCashierView()
        {
            SimulateLogin(new User(1, "Test", "Cashier"));

            CashierViewModel _cashierViewModel = (CashierViewModel)_mainViewModel.CurrentViewModel;

            _orderRepository.Setup(o => o.CreateOrder(It.IsAny<Order>())).Returns(1);
            _orderRepository.Setup(o => o.GetOrderByOrderNumber(1)).Returns(_testOrder);
            _orderRepository.Setup(o => o.DeleteOrder(1)).Returns(true);

            _orderRepository.Setup(o => o.GetCollectionTimes()).Returns((true, new() { "Test" }));
            _orderRepository.Setup(o => o.FetchEligiblePromotions(It.IsAny<decimal>())).Returns([]);

            _cashierViewModel.CurrentOrderItems = _testProducts;

            _cashierViewModel.CheckoutCommand.Execute(null);

            Assert.That(
                _mainViewModel.CurrentViewModel,
                Is.InstanceOf<CheckoutViewModel>(),
                "OnCheckout should redirect to CheckoutViewModel"
            );

            CheckoutViewModel _checkoutViewModel = (CheckoutViewModel)
                _mainViewModel.CurrentViewModel;

            _checkoutViewModel.BackCommand.Execute(null);

            _orderRepository.Verify(
                o => o.DeleteOrder(1),
                Times.Once(),
                "DeleteOrder should be called once"
            );

            Assert.That(
                _mainViewModel.CurrentViewModel,
                Is.InstanceOf<CashierViewModel>(),
                "OnCheckoutBack should redirect to CashierViewModel"
            );

            var _newCashierViewModel = (CashierViewModel)_mainViewModel.CurrentViewModel;

            Assert.That(
                _newCashierViewModel.CurrentOrderItems,
                Is.EquivalentTo(_testProducts),
                "CashierView CurrentOrderItems should contain products from the deleted order"
            );
        }

        // Ensure logging out redirects to login view (We only need to test this once as it's called by an event handler)
        [Test]
        public void OnSessionChanged_NavigatesToLoginViewModel_OnLogout()
        {
            SimulateLogin(new User(1, "Test", "Cashier"));

            Assert.That(
                _mainViewModel.CurrentViewModel,
                Is.InstanceOf<CashierViewModel>(),
                "Login should redirect to correct view"
            );

            // We simulate the act of logging out by having an empty role
            SimulateLogin(new User(1, "Test", ""));

            Assert.That(
                _mainViewModel.CurrentViewModel,
                Is.InstanceOf<LoginViewModel>(),
                "Logout should redirect to login view"
            );
        }

        // We ensure that the cashier view model correctly navigates to OrderView
        [Test]
        public void OnCollection_NavigatesToOrderView()
        {
            SimulateLogin(new User(1, "Test", "Cashier"));

            CashierViewModel _cashierViewModel = (CashierViewModel)_mainViewModel.CurrentViewModel;

            _cashierViewModel.CollectionCommand.Execute(null);

            Assert.That(
                _mainViewModel.CurrentViewModel,
                Is.InstanceOf<OrderViewModel>(),
                "OnCollection should navigate to Order View"
            );

            _orderRepository.Verify(
                o => o.GetCollectionOrders(),
                Times.Once(),
                "Collection orders should be retrieved on navigation"
            );
        }

        // Ensure that when a Driver completes a delivery that has not been paid we navigate to the CheckoutView
        [Test]
        public void OnCompleteOrder_NavigatestoCheckout_Driver()
        {
            Customer testCustomer = new Customer
            {
                ID = 1,
                Name = "Test",
                Postcode = "TA6 5NN",
                StreetAddress = "Street",
                House = "10",
                PhoneNumber = "012345678900",
            };

            _customerRepository.Setup(c => c.GetCustomerByID(1)).Returns(testCustomer);

            Order unpaidDeliveryOrder = new Order
            {
                ID = 1,
                OrderStatus = "Out For Delivery",
                Paid = false,
                CustomerID = 1,
                OrderProducts = _testProducts,
                OrderType = "Delivery",
                DeliveryFee = (decimal)2,
            };

            _orderRepository
                .Setup(o => o.GetDeliveryOrders())
                .Returns(new ObservableCollection<Order> { unpaidDeliveryOrder });
            _orderRepository.Setup(o => o.GetDeliveryTime()).Returns((true, "Test"));
            _orderRepository.Setup(o => o.CompleteOrder(1)).Returns(true);
            _orderRepository.Setup(o => o.GetOrderByOrderNumber(1)).Returns(unpaidDeliveryOrder);

            SimulateLogin(new User(1, "Test", "Driver"));

            OrderViewModel _orderViewModel = (OrderViewModel)_mainViewModel.CurrentViewModel;

            _orderViewModel.CompleteOrderCommand.Execute(1);

            Assert.That(
                _mainViewModel.CurrentViewModel,
                Is.InstanceOf<CheckoutViewModel>(),
                "Completing Delivery of an unpaid order should redirect to Checkout"
            );

            CheckoutViewModel checkoutViewModel = (CheckoutViewModel)
                _mainViewModel.CurrentViewModel;

            checkoutViewModel.CashCommand.Execute(null);
            checkoutViewModel.CompleteOrderCommand.Execute(null);

            Assert.That(
                _mainViewModel.CurrentViewModel,
                Is.InstanceOf<OrderViewModel>(),
                "Payment should redirect back to order view"
            );
        }
    }
}
