using Accessibility;
using Moq;
using NuGet.Frameworks;
using NUnit.Framework;
using PizzaShed;
using PizzaShed.Behaviours;
using PizzaShed.Model;
using PizzaShed.Services.Data;
using PizzaShed.ViewModels;
using PizzaShed.Views;
using PizzaShed.Views.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace PizzaShedTests.System
{    

    // This test will go through the full workflow of the application
    // for a collection order ensuring the application behaves as expected

    // It needs to be run on a clean instance of the application (Run init_db.sql)
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class CollectionOrderTest
    {
        // Define our session object
        private ISession _session;

        // Define our Database Manager Singleton
        private readonly IDatabaseManager _dbManager = DatabaseManager.Instance;
        // Define our Dependencies
        private IUserRepository _userRepository;
        private IOrderRepository _orderRepository;
        private IProductRepository<Product> _productRepository;
        private IProductRepository<Topping> _toppingRepository;
        private ICustomerRepository _customerRepository;

        // Define our View Models
        private MainViewModel _mainViewModel;
        private LoginViewModel _loginViewModel = default!;
        private CashierViewModel _cashierViewModel = default!;
        private CheckoutViewModel _checkoutViewModel = default!;
        private OrderViewModel _orderViewModel = default!;

        [SetUp]
        public void SetUp()
        {
            _session = new Session();

            _userRepository = new UserRepository(_dbManager);
            _orderRepository = new OrderRepository(_dbManager);
            _productRepository = new ProductRepository(_dbManager);
            _toppingRepository = new ToppingRepository(_dbManager);
            _customerRepository = new CustomerRepository(_dbManager);

            _mainViewModel = new MainViewModel(
                                    _session,
                                    _userRepository,
                                    _productRepository,
                                    _toppingRepository,
                                    _orderRepository,
                                    _customerRepository);
        }

        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            var app = (App)Application.Current;

            if (app == null)
            {
                app = new App();
                app.InitializeComponent();
            }
        }

        // Helper function - waits for UI to update before proceeding
        private void UpdateUI()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.ApplicationIdle,
                new DispatcherOperationCallback(_ => { frame.Continue = false; return null; }),
                null);
            Dispatcher.PushFrame(frame);
        }

        // Test Data
        // Collection: Large Pepperoni + Garlic Mayo dip; Card; no discount.
        [Test]
        public void CollectionOrder_SystemTest()
        {
            // We declare our expected results as constants
            const int expectedItemCount = 2;
            const string expectedCost = "£13.69";
            const string expectedVAT = "£2.74";            
            
            // We start the application
            var mainWindow = new MainWindow
            {
                DataContext = _mainViewModel
            };
            mainWindow.Show();

            // We ensure that the application starts with the "Login" view
            Assert.That(
                _mainViewModel.CurrentViewModel,
                Is.TypeOf<LoginViewModel>(),
                "Application should start with the login view");

            _loginViewModel = (LoginViewModel)_mainViewModel.CurrentViewModel;

            // We create a variable to store the UI 
            var loginView = mainWindow.FindDescendant<PizzaShed.Views.Pages.LoginView>();
            Assert.That(
                loginView,
                Is.Not.Null,
                "LoginView should not be null");

            // We will login with credentials from the live database using the UI buttons            
            var zeroButton = loginView.Zero;
            for (int i = 0; i < 3; i++)
            {
                zeroButton.Command.Execute(zeroButton.CommandParameter);
            }
            var oneButton = loginView.One;
            oneButton.Command.Execute(oneButton.CommandParameter);

            UpdateUI();

            // Ensure login was successful and role is correct
            Assert.That(
                _session.IsLoggedIn,
                Is.True,
                "Session should be updated on successful login");
            Assert.That(
                _session.UserRole,
                Is.EqualTo("Cashier"),
                "User role should be cashier");

            // Ensure the current view model is correct
            Assert.That(
                _mainViewModel.CurrentViewModel,
                Is.TypeOf<CashierViewModel>(),
                "Current view model should be cashier view model");
            _cashierViewModel = (CashierViewModel)_mainViewModel.CurrentViewModel;

            // Ensure the current view is correct
            var cashierView = mainWindow.FindDescendant<PizzaShed.Views.Pages.CashierView>();
            Assert.That(
                cashierView,
                Is.Not.Null,
                "The main window should now display the Cashier View"
                );

            // Now we build our test order

            // We navigate to categories using the UI Buttons

            // Since we use ItemsControl to generate our menu buttons 
            // we execute the AddToOrder command from the values stored in the ViewModel
            var pizzaCategoryButton = cashierView.Pizza;
            pizzaCategoryButton.Command.Execute(pizzaCategoryButton.CommandParameter);

            // Ensure we are in the correct category and size buttons are visible
            Assert.That(
                _cashierViewModel.SelectedCategory,
                Is.EqualTo("Pizza"),
                "Selected category should be 'Pizza'");            
            Assert.That(
                cashierView.Small.Visibility, 
                Is.EqualTo(Visibility.Visible),
                "Small button should be visible");
            Assert.That(
                cashierView.Medium.Visibility, 
                Is.EqualTo(Visibility.Visible),
                "Medium button should be visible");
            Assert.That(
                cashierView.Large.Visibility, 
                Is.EqualTo(Visibility.Visible),
                "Large button should be visible");

            // Change the size selection
            var largeButton = cashierView.Large;
            largeButton.Command.Execute(largeButton.CommandParameter);

            // Check that the menu has reflected the size change
            Assert.That(
                _cashierViewModel.CurrentProductMenu.All(
                    p => p.SizeName == "Large" && p.Category == "Pizza"),
                Is.True,
                "Product menu should only contain Large Pizzas");

            Product largePepperoni = _cashierViewModel
                .CurrentProductMenu
                .First(p => 
                p.Name == "Pepperoni");

            // Add the product to the current order
            largePepperoni.AddOrderItemCommand.Execute(largePepperoni);

            // Navigate to the Side / Dip category
            var sideCategory = cashierView.Side;
            sideCategory.Command.Execute(sideCategory.CommandParameter);

            // Ensure that the menu reflected the category change
            Assert.That(
                _cashierViewModel
                    .CurrentProductMenu
                    .All(p => p.Category == "Side"
                    || p.Category == "Dip"),
                Is.True,
                "Product menu should only contain sides or dips");

            
            // Add the product to the current order
            Product garlicMayo = _cashierViewModel
                .CurrentProductMenu
                .First(p =>
                p.Name == "Garlic Mayo"
                && p.SizeName == "60ml"
                && p.Category == "Dip");

            garlicMayo.AddOrderItemCommand.Execute(garlicMayo);

            // Ensure the number of items in the order and total price is as expected
            Assert.That(
                _cashierViewModel.CurrentOrderItems.Count,
                Is.EqualTo(expectedItemCount),
                "CashierViewModel - The number of items in the order should match the expected count");
            Assert.That(
                _cashierViewModel.OrderTotal,
                Is.EqualTo(expectedCost),
                "CashierViewModel - Order total should match the expected cost");


            Assert.That(cashierView.OrderItems.Items.Count,
                Is.EqualTo(expectedItemCount),
                "CashierView - The number of items in the order should match the expected count");
            Assert.That(
                cashierView.OrderTotal.Text,
                Is.EqualTo(expectedCost),
                "CashierView - Order total should match the expected cost");

            // Proceed to checkout
            var checkoutButton = cashierView.Checkout;
            checkoutButton.Command.Execute(checkoutButton.CommandParameter);

            UpdateUI();

            // Ensure the current ViewModel has been updated
            Assert.That(
                _mainViewModel.CurrentViewModel,
                Is.TypeOf<CheckoutViewModel>(),
                "Current ViewModel should be CheckoutViewModel");
            _checkoutViewModel = (CheckoutViewModel)_mainViewModel.CurrentViewModel;

            // Ensure the View has been updated
            var checkoutView = mainWindow.FindDescendant<PizzaShed.Views.Pages.CheckoutView>();
            Assert.That(
                checkoutView,
                Is.Not.Null,
                "Current View should be CheckoutView");

            int orderId = _checkoutViewModel.OrderID;

            // Ensure the Checkout ViewModel has the correct information
            Assert.That(
                _checkoutViewModel.AcceptOrder,
                Is.True,
                "Order should be accepted");
            Assert.That(
                _checkoutViewModel.IsCollection,
                Is.True,
                "Order should be of type collection");
            Assert.That(
                _checkoutViewModel.CurrentOrder.Count,
                Is.EqualTo(expectedItemCount),
                "CheckoutViewModel - The number of items in the current order should match the expected count");
            Assert.That(
                _checkoutViewModel.TotalPriceValue,
                Is.EqualTo(expectedCost),
                "CheckoutViewModel - Total Price should match the expected cost");
            Assert.That(
                _checkoutViewModel.VATValue,
                Is.EqualTo(expectedVAT),
                "CheckoutViewModel - VAT should match the expected amount");

            // Make sure the CheckoutView displays correct information
            Assert.That(
                checkoutView.Cash.Visibility,
                Is.EqualTo(Visibility.Visible),
                "Cash payment button should be visible");
            Assert.That(
                checkoutView.Card.Visibility,
                Is.EqualTo(Visibility.Visible),
                "Card payment button should be visible");
            Assert.That(
                checkoutView.Notes.Visibility,
                Is.EqualTo(Visibility.Visible),
                "Order Notes text box should be visible for a new order");
            Assert.That(
                checkoutView.CollectionTime.Visibility,
                Is.EqualTo(Visibility.Visible),
                "Collection Time drop down should be visible");
            Assert.That(
                checkoutView.OrderTotalValue.Text,
                Is.EqualTo(expectedCost),
                "CheckoutView - Order Total should match the expected cost");
            Assert.That(
                checkoutView.VATValue.Text,
                Is.EqualTo(expectedVAT),
                "CheckoutView - VAT value should match the expected amount");

            // Proceed with order payment
            var cardButton = checkoutView.Card;
            cardButton.Command.Execute(cardButton.CommandParameter);

            UpdateUI();

            // Ensure the payment window behaves as expected
            var paymentWindow = Application.Current.Windows
                .OfType<PaymentWindow>()
                .First(w => w.IsVisible);

            Assert.That(
                paymentWindow,
                Is.Not.Null,
                "Payment Window should not be null");

            // Find the UI in the payment window
            var paymentView = paymentWindow.FindDescendant<PizzaShed.Views.Pages.PaymentPresentView>();
            Assert.That(
                paymentView,
                Is.Not.Null,
                "Payment view should not be null");

            // Ensure the MainViewModel has the correct payment ViewModel
            Assert.That(
                _mainViewModel.PaymentViewModel,
                Is.TypeOf<PaymentPresentViewModel>(),
                "PaymentViewModel should be of type PaymentPresentViewModel");

            PaymentPresentViewModel paymentViewModel = (PaymentPresentViewModel)_mainViewModel.PaymentViewModel;

            // Check that the payment view / viewmodel display the total cost by default
            Assert.That(
                paymentView.TotalBox.Text,
                Is.EqualTo(expectedCost),
                "PaymentView Total should be the total order cost by default");
            Assert.That(
                paymentViewModel.Total,
                Is.EqualTo(expectedCost),
                "PaymentViewModel Total should be the total order cost by default");

            var completePaymentButton = paymentView.Submit;
            completePaymentButton.Command.Execute(completePaymentButton.CommandParameter);

            UpdateUI();

            // Ensure the payment window disappears after payment is made
            Assert.That(
                paymentWindow.Visibility,
                Is.EqualTo(Visibility.Hidden),
                "Payment window should not be visible after payment");

            // Ensure the checkout viewmodel has been updated
            Assert.That(
                _checkoutViewModel.IsPaid,
                Is.True,
                "IsPaid should be true after successful payment");
            Assert.That(
                _checkoutViewModel.TotalPriceValue,
                Is.EqualTo("£0.00"),
                "CheckoutViewModel - Order Total should reflect payments");
            Assert.That(
                _checkoutViewModel.VATValue,
                Is.EqualTo(expectedVAT),
                "CheckoutViewModel - VAT should not change even after payment");

            // Ensure the checkout View has been updated
            Assert.That(
                checkoutView.OrderTotalValue.Text,
                Is.EqualTo("£0.00"),
                "CheckoutView - Order Total should reflect payments");
            Assert.That(
                checkoutView.VATValue.Text,
                Is.EqualTo(expectedVAT),
                "CheckoutView - VAT should not change even after payment");
            Assert.That(
                checkoutView.Complete.Visibility,
                Is.EqualTo(Visibility.Visible),
                "Complete order button should be visible after payment");

            // Complete the order
            var completeButton = checkoutView.Complete;
            completeButton.Command.Execute(completeButton.CommandParameter);

            UpdateUI();

            // Check that we successfully navigate back to the cashier view
            Assert.That(
                _mainViewModel.CurrentViewModel,
                Is.TypeOf<CashierViewModel>(),
                "After payment we should be redirected to the cashier view");
            
            _cashierViewModel = (CashierViewModel)_mainViewModel.CurrentViewModel;

            cashierView = mainWindow.FindDescendant<PizzaShed.Views.Pages.CashierView>();
            Assert.That(
                cashierView,
                Is.Not.Null,
                "Main Window should display the cashier view after successful payment");

            // Check the order has appeared in the OrderView(Cashier) as a new order
            var collectionButton = cashierView.Collections;
            collectionButton.Command.Execute(collectionButton.CommandParameter);

            UpdateUI();

            // Make sure the Current View has updated
            Assert.That(
                _mainViewModel.CurrentViewModel,
                Is.TypeOf<OrderViewModel>(),
                "Current ViewModel should be of type OrderViewModel");
            _orderViewModel = (OrderViewModel)_mainViewModel.CurrentViewModel;  

            var orderView = mainWindow.FindDescendant<PizzaShed.Views.Pages.OrderView>();
            Assert.That(
                orderView,
                Is.Not.Null,
                "Order View should not be null");

            // Check the order exists in the "In Progress" orders section
            Assert.That(
                _orderViewModel.IsCashier,
                Is.True,
                "Order ViewModel should reflect the current role (Cashier)");
            Assert.That(
                _orderViewModel.NewOrders.Any(o => o.ID == orderId),
                Is.True,
                "OrderViewModel should contain our new order (Cashier)");
            Assert.That(
                orderView.NewOrders.Items.OfType<Order>().Any(o => o.ID == orderId),
                Is.True,
                "OrderView - New orders should contain our new order (Cashier)");

            // Logout 
            var logoutButton = orderView.LogoutButton;
            logoutButton.Command.Execute(logoutButton.CommandParameter);

            UpdateUI();

            // Ensure we are redirected to the login view
            Assert.That(
                _mainViewModel.CurrentViewModel,
                Is.TypeOf<LoginViewModel>(),
                "Logout should redirect to login (Cashier)");
            _loginViewModel = (LoginViewModel)_mainViewModel.CurrentViewModel;

            loginView = mainWindow.FindDescendant<PizzaShed.Views.Pages.LoginView>();
            Assert.That(
                loginView,
                Is.Not.Null,
                "Login View should not be null (Cashier)");

            // Login as Pizzaiolo
            zeroButton = loginView.Zero;
            for (int i = 0; i < 3; i++ )
            {
                zeroButton.Command.Execute(zeroButton.CommandParameter);
            }
            var twoButton = loginView.Two;
            twoButton.Command.Execute(twoButton.CommandParameter);

            UpdateUI();

            // Ensure login was successful
            Assert.That(
                _session.IsLoggedIn,
                Is.True,
                "Session should reflect successful login (Pizzaiolo)");
            Assert.That(
                _session.UserRole,
                Is.EqualTo("Pizzaiolo"),
                "Session should reflect the user's role correctly (Pizzaiolo)");

            // Ensure the UI updated
            Assert.That(
                _mainViewModel.CurrentViewModel,
                Is.TypeOf<OrderViewModel>(),
                "Current View model should be Order View (Pizzaiolo)");
            _orderViewModel = (OrderViewModel)_mainViewModel.CurrentViewModel;

            orderView = mainWindow.FindDescendant<PizzaShed.Views.Pages.OrderView>();
            Assert.That(
                orderView,
                Is.Not.Null,
                "Order view should not be null (Pizzaiolo)");
            Assert.That(
                _orderViewModel.IsCook,
                Is.True,
                "OrderViewModel should reflect the current role (Pizzaiolo)");


            // Ensure that the order exists in new orders
            Assert.That(
                _orderViewModel.NewOrders.Any(o => o.ID == orderId),
                Is.True,
                "OrderViewModel - New orders should contain our new order (Pizzaiolo)");

            Assert.That(
                orderView.NewOrders.Items.OfType<Order>().Any(o => o.ID == orderId),
                Is.True,
                "OrderView - New orders should contain our new order (Pizzaiolo)");

            // Check the order status
            Order testOrder = _orderViewModel.NewOrders.First(o => o.ID == orderId);
            Assert.That(
                testOrder.OrderStatus,
                Is.EqualTo("New"),
                "Order Status should be 'New'");


            // Start preparing the order
            _orderViewModel.CompleteOrderCommand.Execute(testOrder.ID);

            UpdateUI();            

            // Check the UI updates as expected and order status is updated
            Assert.That(
                _orderViewModel.NewOrders.Any(o => o.ID == orderId),
                Is.False,
                "OrderViewModel - New orders should not contain our order");
            Assert.That(
                _orderViewModel.ReadyOrders.Any(o => o.ID == orderId),
                Is.True,
                "OrderViewModel - ReadyOrders should contain the order we are preparing");
            Assert.That(
                orderView.NewOrders.Items.OfType<Order>().Any(o => o.ID == orderId),
                Is.False,
                "OrderView - New orders should not contain our order");
            Assert.That(
                orderView.ReadyOrders.Items.OfType<Order>().Any(o => o.ID == orderId),
                Is.True,
                "OrderView - ReadyOrders should contain the order we are preparing");

            testOrder = _orderViewModel.ReadyOrders.First(o => o.ID == orderId);

            Assert.That(
                testOrder.OrderStatus,
                Is.EqualTo("Preparing"),
                "Order Status should be updated on prep");

            // Finish preparing the order
            _orderViewModel.CompleteOrderCommand.Execute(testOrder.ID);

            UpdateUI();

            // Check the UI updates as expected and order status is updated
            Assert.That(
                _orderViewModel.NewOrders.Any(o => o.ID == orderId),
                Is.False,
                "OrderViewModel - New orders should contain no items after prep");
            Assert.That(
                _orderViewModel.ReadyOrders.Any(o => o.ID == orderId),
                Is.False,
                "OrderViewModel - ReadyOrders should contain no items after prep");
            Assert.That(
                orderView.NewOrders.Items.OfType<Order>().Any(o => o.ID == orderId),
                Is.False,
                "OrderView - New orders should contain no items after  prep");
            Assert.That(
                orderView.ReadyOrders.Items.OfType<Order>().Any(o => o.ID == orderId),
                Is.False,
                "OrderView - ReadyOrders should contain no orders after  prep");


            // Log out

            logoutButton = orderView.LogoutButton;
            logoutButton.Command.Execute(logoutButton.CommandParameter);

            UpdateUI();

            // Ensure we are redirected to the login view
            Assert.That(
                _mainViewModel.CurrentViewModel,
                Is.TypeOf<LoginViewModel>(),
                "Logout should redirect to login (Pizzaiolo)");
            _loginViewModel = (LoginViewModel)_mainViewModel.CurrentViewModel;

            loginView = mainWindow.FindDescendant<PizzaShed.Views.Pages.LoginView>();
            Assert.That(
                loginView,
                Is.Not.Null,
                "Login View should not be null (Pizzaiolo)");

            // Log in as Cashier again and navigate to collections

            zeroButton = loginView.Zero;
            for (int i = 0; i < 3; i++)
            {
                zeroButton.Command.Execute(zeroButton.CommandParameter);
            }
            oneButton = loginView.One;
            oneButton.Command.Execute(oneButton.CommandParameter);

            UpdateUI();

            // Ensure login was successful and role is correct
            Assert.That(
                _session.IsLoggedIn,
                Is.True,
                "Session should be updated on successful login");
            Assert.That(
                _session.UserRole,
                Is.EqualTo("Cashier"),
                "User role should be cashier");

            // Ensure the current view model is correct
            Assert.That(
                _mainViewModel.CurrentViewModel,
                Is.TypeOf<CashierViewModel>(),
                "Current view model should be cashier view model");
            _cashierViewModel = (CashierViewModel)_mainViewModel.CurrentViewModel;

            // Ensure the current view is correct
            cashierView = mainWindow.FindDescendant<PizzaShed.Views.Pages.CashierView>();
            Assert.That(
                cashierView,
                Is.Not.Null,
                "The main window should now display the Cashier View"
                );

            collectionButton = cashierView.Collections;
            collectionButton.Command.Execute(collectionButton.CommandParameter);

            UpdateUI();

            // Ensure we have been redirected to the order view
            Assert.That(
               _mainViewModel.CurrentViewModel,
               Is.TypeOf<OrderViewModel>(),
               "Current ViewModel should be of type OrderViewModel (Cashier - 2)");
            _orderViewModel = (OrderViewModel)_mainViewModel.CurrentViewModel;

            orderView = mainWindow.FindDescendant<PizzaShed.Views.Pages.OrderView>();
            Assert.That(
                orderView,
                Is.Not.Null,
                "Order View should not be null");

            // Check the order exists in the "In Progress" orders section
            Assert.That(
                _orderViewModel.IsCashier,
                Is.True,
                "OrderViewModel - should reflect the current role (Cashier - 2)");
            Assert.That(
                _orderViewModel.NewOrders.Any(o => o.ID == orderId),
                Is.False,
                "OrderViewModel - NewOrders should not contain our order (Cashier - 2)");
            Assert.That(
                orderView.NewOrders.Items.OfType<Order>().Any(o => o.ID == orderId),
                Is.False    ,
                "OrderView - NewOrders should not contain our order (Cashier - 2)");
            Assert.That(
                _orderViewModel.ReadyOrders.Any(o => o.ID == orderId),
                Is.True,
                "OrderViewModel - ReadyOrders should have the prepared order (Cashier - 2)");
            Assert.That(
                orderView.ReadyOrders.Items.OfType<Order>().Any(o => o.ID == orderId),
                Is.True,
                "OrderView - ReadyOrders should have the prepared order (Cashier - 2)");


            testOrder = _orderViewModel.ReadyOrders.First(o => o.ID == orderId);

            // Check the order status
            Assert.That(
                testOrder.OrderStatus,
                Is.EqualTo("Order Ready"),
                "Order status should be 'Order Ready'");

            // Complete the order 
            _orderViewModel.CompleteOrderCommand.Execute(testOrder.ID);

            UpdateUI();

            // Ensure the UI updates
            Assert.That(
                _orderViewModel.NewOrders.Any(o => o.ID == orderId),
                Is.False,
                "OrderViewModel - NewOrders should contain no orders on completion  (Cashier - 2)");
            Assert.That(
                orderView.NewOrders.Items.OfType<Order>().Any(o => o.ID == orderId),
                Is.False,
                "OrderView - NewOrders should not contain our order on completion  (Cashier - 2)");
            Assert.That(
                _orderViewModel.ReadyOrders.Any(o => o.ID == orderId),
                Is.False,
                "OrderViewModel - ReadyOrders should not contain our order on completion  (Cashier - 2)");
            Assert.That(
                orderView.ReadyOrders.Items.OfType<Order>().Any(o => o.ID == orderId),
                Is.False,
                "OrderView - ReadyOrders should not contain our order on completion (Cashier - 2)");
            
            mainWindow.Hide();
        }
    }
}
