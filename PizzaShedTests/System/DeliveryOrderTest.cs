using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using PizzaShed.Views.Pages;

namespace PizzaShedTests.System
{

    // This test will go through the full workflow of the application
    // for a deal order ensuring the application behaves as expected

    // It needs to be run on a clean instance of the application (Run init_db.sql)
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class DeliveryOrderTest
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
        private CustomerViewModel _customerViewModel = default!;

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
        //  Delivery: Medium Doner Delight half & half with Veggie Supreme + 2 dips; Delivery ≤2 miles; Cash on delivery.
        [Test]
        public void DealOrder_SystemTest()
        {
            // We declare our expected results as constants
            const int expectedItemCount = 3;
            const string expectedCostNoDelivery = "£12.39";
            const string expectedCostWithDelivery = "£14.39";
            const string expectedDeliveryCost = "£2.00";
            const string expectedVAT = "£2.88";

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
            var cashierView = mainWindow.FindDescendant<CashierView>();
            Assert.That(
                cashierView,
                Is.Not.Null,
                "The main window should now display the Cashier View"
                );

            // Now we build our test order

            // We navigate to categories using the UI Buttons

            // Since we use ItemsControl to generate our menu buttons 
            // we execute the AddToOrder command from the values stored in the ViewModel            

            // Ensure we are in the correct category (Deal by default) 
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
            var mediumButton = cashierView.Medium;
            mediumButton.Command.Execute(mediumButton.CommandParameter);

            // Check that the menu has reflected the size change
            Assert.That(
                _cashierViewModel.CurrentProductMenu.All(
                    p => p.SizeName == "Medium" && p.Category == "Pizza"),
                Is.True,
                "Product menu should only contain Medium Pizzas");

            // Select the half and half button
            var halfButton = cashierView.Half;
            halfButton.Command.Execute(halfButton.CommandParameter);

            // Check that half and half is set in the view model
            Assert.That(
                _cashierViewModel.IsHalfAndHalf,
                Is.True,
                "IsHalfAndHalf should be true");

            Product mediumDoner = _cashierViewModel
                .CurrentProductMenu
                .First(p =>
                p.Name == "Doner Delight");

            // Add the first half to the current order
            mediumDoner.AddOrderItemCommand.Execute(mediumDoner);

            // Add the second half
            Product mediumVeggie = _cashierViewModel
                .CurrentProductMenu
                .First(p =>
                p.Name == "Veggie Supreme");

            mediumVeggie.AddOrderItemCommand.Execute(mediumVeggie);

            // Check that we only have one half and half pizza in the current order
            Assert.That(
                _cashierViewModel.CurrentOrderItems.Count,
                Is.EqualTo(1),
                "CashierViewModel - Half and Half should only add one item");
            Assert.That(
                cashierView.OrderItems.Items,
                Has.Count.EqualTo(1),
                "CashierView - Half and Half shoul only add one item");


            Product halfPizza = _cashierViewModel.CurrentOrderItems.First();
            // Check that the pizza name and price was set correctly
            Assert.That(
                halfPizza.Name,
                Is.EqualTo($"{mediumDoner.Name} / {mediumVeggie.Name}"),
                "Half and Half Pizzas name should reflect the two halves");

            Assert.That(
                halfPizza.Price,
                Is.EqualTo(mediumDoner.Price),
                "Half and Half Pizzas should have the price of the more expensive half");

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

            // Add two dips
            garlicMayo.AddOrderItemCommand.Execute(garlicMayo);
            garlicMayo.AddOrderItemCommand.Execute(garlicMayo);

            // Ensure the number of items in the order and total price is as expected
            Assert.That(
                _cashierViewModel.CurrentOrderItems.Count,
                Is.EqualTo(expectedItemCount),
                "CashierViewModel - The number of items in the order should match the expected count");
            Assert.That(
                _cashierViewModel.OrderTotal,
                Is.EqualTo(expectedCostNoDelivery),
                "CashierViewModel - Order total should match the expected cost");


            Assert.That(cashierView.OrderItems.Items.Count,
                Is.EqualTo(expectedItemCount),
                "CashierView - The number of items in the order should match the expected count");
            Assert.That(
                cashierView.OrderTotal.Text,
                Is.EqualTo(expectedCostNoDelivery),
                "CashierView - Order total should match the expected cost");

            // Select delivery button
            var deliveryButton = cashierView.Delivery;
            deliveryButton.Command.Execute(deliveryButton.CommandParameter);

            // Proceed to checkout
            var checkoutButton = cashierView.Checkout;
            checkoutButton.Command.Execute(checkoutButton.CommandParameter);

            UpdateUI();

            // Ensure the ViewModel hash been updated and we have been redirected to CustomerView

            Assert.That(
                _mainViewModel.CurrentViewModel,
                Is.TypeOf<CustomerViewModel>(),
                "Current ViewModel should be CustomerViewModel");
            _customerViewModel = (CustomerViewModel)_mainViewModel.CurrentViewModel;

            var customerView = mainWindow.FindDescendant<PizzaShed.Views.Pages.CustomerView>();
            Assert.That(
                customerView,
                Is.Not.Null,
                "Main Window should display the CustomerView");

            // We will use the auto-fill functionality for an existing customer that is within 2 miles
            _customerViewModel.NameSearch += "A";

            // Make sure the suggestion exists in the ViewModel and the UI
            Assert.That(
                _customerViewModel.CustomerSuggestion.Count,
                Is.EqualTo(1),
                "CustomViewModel - Customer suggestion should contain a value");

            Assert.That(
                customerView.Suggestions.Items,
                Has.Count.EqualTo(1),
                "CustomerView - Customer Suggestion should contain a value");

            Customer suggestedCustomer = _customerViewModel.CustomerSuggestion.First();
            // Select the suggested customer
            _customerViewModel.ProxyCustomer = suggestedCustomer;

            // Check that CurrentCustomer has been updated
            Assert.That(
                _customerViewModel.CurrentCustomer,
                Is.EqualTo(suggestedCustomer),
                "Current Customer should be equal to the suggested customer"
                );

            // Check the auto-fill worked in the UI
            Assert.That(
                customerView.NameBox.Text,
                Is.EqualTo(suggestedCustomer.Name),
                "Auto-fill should update the Customers name");
            Assert.That(
                customerView.NumberBox.Text,
                Is.EqualTo(suggestedCustomer.PhoneNumber),
                "Auto-fill should update the Customers number");
            Assert.That(
                customerView.AddressBox.Text,
                Is.EqualTo(suggestedCustomer.StreetAddress),
                "Auto-fill should update the Customers Address");
            Assert.That(
                customerView.FlatBox.Text,
                Is.EqualTo(suggestedCustomer.Flat == null ? string.Empty : suggestedCustomer.Flat),
                "Auto-fill should update the Customers Flat");
            Assert.That(
                customerView.HouseBox.Text,
                Is.EqualTo(suggestedCustomer.House),
                "Auto-fill should update the Customers House Number");
            Assert.That(
                customerView.PostcodeBox.Text,
                Is.EqualTo(suggestedCustomer.Postcode),
                "Auto-fill should update the Customers Postcode");
            Assert.That(
                customerView.NotesBox.Text,
                Is.EqualTo(suggestedCustomer.DeliveryNotes == null ? string.Empty : suggestedCustomer.DeliveryNotes),
                "Auto-fill should update the Customers Delivery Notes");


            // Continue with the checkout process

            checkoutButton = customerView.Checkout;
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
                _checkoutViewModel.IsDelivery,
                Is.True,
                "Order should be of type delivery");
            Assert.That(
                _checkoutViewModel.CurrentOrder.Count,
                Is.EqualTo(expectedItemCount),
                "CheckoutViewModel - The number of items in the current order should match the expected count");
            Assert.That(
                _checkoutViewModel.TotalPriceValue,
                Is.EqualTo(expectedCostWithDelivery),
                "CheckoutViewModel - Total Price should match the expected cost");
            Assert.That(
                _checkoutViewModel.DeliveryValue,
                Is.EqualTo(expectedDeliveryCost),
                "CheckoutViewModel - Delivery Cost should match the expected cost");

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
                Is.EqualTo(Visibility.Collapsed),
                "Collection Time drop down should not be visible");
            Assert.That(
                checkoutView.DeliveryValue.Visibility,
                Is.EqualTo(Visibility.Visible),
                "Expected delivery time should be visible");
            Assert.That(
                checkoutView.OrderTotalValue.Text,
                Is.EqualTo(expectedCostWithDelivery),
                "CheckoutView - Order Total should match the expected cost");
            Assert.That(
                checkoutView.DeliveryValue.Text,
                Is.EqualTo(expectedDeliveryCost),
                "CheckoutView - Delivery Cost should match the expected cost");
           
                   

            // Proceed with order payment
            var cashButton = checkoutView.Cash;
            cashButton.Command.Execute(cashButton.CommandParameter);

            UpdateUI();

            // Ensure the payment window does not appear
            var paymentWindowVisible = Application.Current.Windows
                .OfType<PaymentWindow>()
                .Any(w => w.IsVisible);

            Assert.That(
                paymentWindowVisible,
                Is.False,
                "Payment Window should not be visible for cash on delivery");
                        

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
                "Main window should display the cashier view after order is finished");
            
            // Logout 
            var logoutButton = cashierView.Logout;
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
            for (int i = 0; i < 3; i++)
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

            var orderView = mainWindow.FindDescendant<PizzaShed.Views.Pages.OrderView>();
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
                "OrderView - New orders should not contain our  order");
            Assert.That(
                orderView.ReadyOrders.Items.OfType<Order>().Any(o => o.ID == orderId),
                Is.True,
                "OrderView - ReadyOrders should contain the order we are preparing");

            testOrder = _orderViewModel.ReadyOrders.First();

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
                "OrderViewModel - New orders should not contain our order after prep");
            Assert.That(
                _orderViewModel.ReadyOrders.Any(o => o.ID == orderId)   ,
                Is.False,
                "OrderViewModel - ReadyOrders should not contain our order after prep");
            Assert.That(
                orderView.NewOrders.Items.OfType<Order>().Any(o => o.ID == orderId),
                Is.False,
                "OrderView - New orders should not contain our order after prep");
            Assert.That(
                orderView.ReadyOrders.Items.OfType<Order>().Any(o => o.ID == orderId),
                Is.False,
                "OrderView - ReadyOrders should not contain our order after prep");


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

            loginView = mainWindow.FindDescendant<LoginView>();
            Assert.That(
                loginView,
                Is.Not.Null,
                "Login View should not be null (Pizzaiolo)");

            // Log in as Driver and navigate to deliveries

            zeroButton = loginView.Zero;
            for (int i = 0; i < 3; i++)
            {
                zeroButton.Command.Execute(zeroButton.CommandParameter);
            }
            var fourButton = loginView.Four;
            fourButton.Command.Execute(fourButton.CommandParameter);

            UpdateUI();

            // Ensure login was successful and role is correct
            Assert.That(
                _session.IsLoggedIn,
                Is.True,
                "Session should be updated on successful login");
            Assert.That(
                _session.UserRole,
                Is.EqualTo("Driver"),
                "User role should be driver");

            // Ensure the current view model is correct            
            Assert.That(
               _mainViewModel.CurrentViewModel,
               Is.TypeOf<OrderViewModel>(),
               "Current ViewModel should be of type OrderViewModel (Driver)");
            _orderViewModel = (OrderViewModel)_mainViewModel.CurrentViewModel;

            orderView = mainWindow.FindDescendant<PizzaShed.Views.Pages.OrderView>();
            Assert.That(
                orderView,
                Is.Not.Null,
                "Order View should not be null");

            // Check the order exists in the "Ready For Delivery" orders section
            Assert.That(
                _orderViewModel.IsDriver,
                Is.True,
                "OrderViewModel - should reflect the current role (Driver)");
            Assert.That(
                _orderViewModel.NewOrders.Any(o => o.ID == orderId),
                Is.True,
                "OrderViewModel - NewOrders should have our new order (Driver)");
            Assert.That(
                orderView.NewOrders.Items.OfType<Order>().Any(o => o.ID == orderId),
                Is.True,
                "OrderView - NewOrders should have our new order (Driver)");
            Assert.That(
                _orderViewModel.ReadyOrders.Any(o => o.ID == orderId),
                Is.False,
                "OrderViewModel - ReadyOrder should not have our order (Driver)");
            Assert.That(
                orderView.ReadyOrders.Items.OfType<Order>().Any(o => o.ID == orderId),
                Is.False,
                "OrderView - ReadyOrders should not have our order (Driver)");


            testOrder = _orderViewModel.NewOrders.First(o => o.ID == orderId);

            // Check the order details
            Assert.That(
                testOrder.OrderStatus,
                Is.EqualTo("Order Ready"),
                "Order status should be 'Order Ready'");
            Assert.That(
                testOrder.Paid,
                Is.False,
                "Cash on delivery order should not be marked as paid");
            Assert.That(
                testOrder.Customer.ID,
                Is.EqualTo(suggestedCustomer.ID),
                "Order should have the same customer details");

            // Complete the order 
            _orderViewModel.CompleteOrderCommand.Execute(testOrder.ID);

            UpdateUI();

            
            // Check the UI and order status has updated
            Assert.That(
                _orderViewModel.NewOrders.Any(o => o.ID == orderId),
                Is.False,
                "OrderViewModel - NewOrders should no longer contain our order (Driver)");
            Assert.That(
                orderView.NewOrders.Items.OfType<Order>().Any(o => o.ID == orderId),
                Is.False,
                "OrderView - NewOrders should no longer contain our order (Driver)");
            Assert.That(
                _orderViewModel.ReadyOrders.Any(o => o.ID == orderId),
                Is.True,
                "OrderViewModel - ReadyOrder should now contain our order (Driver)");
            Assert.That(
                orderView.ReadyOrders.Items.OfType<Order>().Any(o => o.ID == orderId),
                Is.True,
                "OrderView - ReadyOrders should now contain our order (Driver)");

            testOrder = _orderViewModel.ReadyOrders.First(o => o.ID == orderId);
            Assert.That(
                testOrder.OrderStatus,
                Is.EqualTo("Out For Delivery"),
                "Order Status should be updated to 'Out For Delivery'");

            // Complete the delivery
            _orderViewModel.CompleteOrderCommand.Execute(testOrder.ID);
            
            UpdateUI();

            // Ensure we navigate to CheckoutView when completing a cash on delivery order            

            Assert.That(
                _mainViewModel.CurrentViewModel,
                Is.TypeOf<CheckoutViewModel>(),
                "Current ViewModel should be CheckoutViewModel after delivering a cash on delivery order");
            _checkoutViewModel = (CheckoutViewModel)_mainViewModel.CurrentViewModel;

            checkoutView = mainWindow.FindDescendant<CheckoutView>();
            Assert.That(
                checkoutView,
                Is.Not.Null,
                "Main Windows should display checkout view after deliverying a cash on delivery order");

            // Ensure the view model has the correct values
            Assert.That(
                _checkoutViewModel.IsPaid,
                Is.False,
                "Unpaid cash on delivery order should not be marked as Paid");
            Assert.That(
                _checkoutViewModel.TotalPriceValue,
                Is.EqualTo(expectedCostWithDelivery),
                "CheckoutViewModel - Total Cost should match the expected cost");
            Assert.That(
                _checkoutViewModel.VATValue,
                Is.EqualTo(expectedVAT),
                "CheckoutViewModel - VAT should match the expected VAT amount");
            Assert.That(
                _checkoutViewModel.DeliveryValue,
                Is.EqualTo(expectedDeliveryCost),
                "CheckoutViewModel - Delivery charge should match the expected delivery cost");

            // Ensure the UI is displaying the correct elements
            Assert.That(
                checkoutView.Card.Visibility,
                Is.EqualTo(Visibility.Collapsed),
                "Card button should not be visible for a Cash on Delivery order");
            Assert.That(
                checkoutView.Promotions.Visibility,
                Is.EqualTo(Visibility.Collapsed),
                "Promotions should not be visible for a Cash on Delivery order");
            Assert.That(
                checkoutView.Notes.Visibility,
                Is.EqualTo(Visibility.Collapsed),
                "Order Notes should not be visible for a Delivered order");
            Assert.That(
                checkoutView.OrderTotalValue.Text,
                Is.EqualTo(expectedCostWithDelivery),
                "CheckoutView - Total Cost should match the expected cost");
            Assert.That(
                checkoutView.VATValue.Text,
                Is.EqualTo(expectedVAT),
                "CheckoutView - VAT should match the expected VAT");
            Assert.That(
                checkoutView.DeliveryValue.Text,
                Is.EqualTo(expectedDeliveryCost),
                "CheckoutView - Delivery charge should match the expected delivery cost");

            // Make sure the cash payment button is visible for the driver
            Assert.That(
                checkoutView.Cash.Visibility,
                Is.EqualTo(Visibility.Visible),
                "Cash button should be visible for Cash on Delivery order");

            // Begin payment
            cashButton = checkoutView.Cash;
            cashButton.Command.Execute(cashButton.CommandParameter);

            UpdateUI();

            // Ensure the payment window shows for the driver
            var paymentWindow = Application
                                    .Current
                                    .Windows
                                    .OfType<PaymentWindow>()
                                    .First(w => w.IsVisible);

            Assert.That(
                paymentWindow,
                Is.Not.Null,
                "Payment Window should be visible on driver completing cash on delivery");

            var paymentView = paymentWindow.FindDescendant<PaymentPresentView>();
            Assert.That(
                paymentView,
                Is.Not.Null,
                "PaymentWindow should display the PaymentPresentView");
            
            Assert.That(
                _mainViewModel.PaymentViewModel,
                Is.TypeOf<PaymentPresentViewModel>(),
                "PaymentViewModel should be of type PaymentPresent");
            PaymentPresentViewModel paymentViewModel = (PaymentPresentViewModel)_mainViewModel.PaymentViewModel;

            // Check that the payment view / viewmodel display the total cost by default
            Assert.That(
                paymentView.TotalBox.Text,
                Is.EqualTo(expectedCostWithDelivery),
                "PaymentView Total should be the total order cost by default");
            Assert.That(
                paymentViewModel.Total,
                Is.EqualTo(expectedCostWithDelivery),
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
            completeButton = checkoutView.Complete;
            completeButton.Command.Execute(completeButton.CommandParameter);

            UpdateUI();

            // Ensure we are redirected back to the OrderView
            Assert.That(
                _mainViewModel.CurrentViewModel,
                Is.TypeOf<OrderViewModel>(),
                "Payment should redirect back to the order view");

            orderView = mainWindow.FindDescendant<OrderView>();
            Assert.That(
                orderView,
                Is.Not.Null,
                "MainWindow should display the OrderView after successful payment");

            mainWindow.Hide();
        }
    }
}

