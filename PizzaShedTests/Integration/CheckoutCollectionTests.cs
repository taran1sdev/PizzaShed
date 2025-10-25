using Accessibility;
using Moq;
using PizzaShed;
using PizzaShed.Model;
using PizzaShed.Services.Data;
using PizzaShed.ViewModels;
using PizzaShed.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media;
using PizzaShed.Behaviours;
using System.Windows.Controls.Primitives;
using System.Windows.Automation.Peers;
using NuGet.Frameworks;

namespace PizzaShedTests.Integration
{
    [Apartment(ApartmentState.STA)]
    [TestFixture]
    public class CheckoutCollectionTests
    {
        private IDatabaseManager _databaseManager = DatabaseManager.Instance;

        private IUserRepository _userRepository;
        private ISession _session;
        private MainViewModel _mainViewModel;
        private CashierViewModel _cashierViewModel;
        private CheckoutViewModel _checkoutViewModel = default!;
        private IProductRepository<Product> _productRepository;
        private IProductRepository<Topping> _toppingRepository;
        private IOrderRepository _orderRepository;

        // Mock dependencies not needed during testing
        private Mock<ICustomerRepository> _customerRepository;

        [SetUp]
        public void SetUp()
        {
            _userRepository = new UserRepository(_databaseManager);
            _session = new Session();
            _productRepository = new ProductRepository(_databaseManager);
            _toppingRepository = new ToppingRepository(_databaseManager);
            _orderRepository = new OrderRepository(_databaseManager);
            _customerRepository = new Mock<ICustomerRepository>();

            _mainViewModel = new MainViewModel(
                                    _session,
                                    _userRepository,
                                    _productRepository,
                                    _toppingRepository,
                                    _orderRepository,
                                    _customerRepository.Object
                                    );

            _session.Login(new User(1, "Test", "Cashier"));

            _cashierViewModel = (CashierViewModel)_mainViewModel.CurrentViewModel;
        }

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

        [OneTimeTearDown]
        public void FixtureTearDown()
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                if (Application.Current is { }) Application.Current.Shutdown();
            });
        }

        private static IEnumerable<TestCaseData> OrderCreationTestCases
        {
            get
            {
                // We use a dictionary to hold our order data
                yield return new TestCaseData(
                    new Dictionary<string, string[][]>
                    {
                        {"Pizza", [["Margherita", "Small"],["Pepperoni", "Medium"]] },
                        {"Kebab", [["Chicken Shish\n(Regular)"]]},
                        {"Drink", [["Coke\n(330ml)"]] },
                        {"Side", [["Garlic Mayo\n(60ml)"], ["Chicken Wings\n(6)"]] }
                    }, 2, 6
                );
                yield return new TestCaseData(
                    new Dictionary<string, string[][]>
                    {
                        {"Pizza", [["Pepperoni", "Large"]] }
                    }, 1, 1
                );
                yield return new TestCaseData(
                    new Dictionary<string, string[][]>
                    {
                        {"Burger", [["Cheese Burger"]]}
                    }, 0, 1
                );
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

        // We already know from Unit tests that Order total is calculated correctly so we can just confirm that checkout displays the same
        [TestCaseSource(nameof(OrderCreationTestCases))]
        public void OrderCreation_NavigatesToCheckout_WithCorrectPromotions_Price_VAT(Dictionary<string, string[][]> orderItems, int promoCount, int itemCount)
        {
            var window = new PizzaShed.Views.Windows.MainWindow
            {
                DataContext = _mainViewModel
            };
            window.Show();

            var cashierView = window.FindDescendant<PizzaShed.Views.Pages.CashierView>();
            Assert.That(cashierView, Is.Not.Null);

            _cashierViewModel = (CashierViewModel)cashierView.DataContext;            
            
            // Here we populate the order in the cashier view
            foreach (string category in orderItems.Keys)
            {
                // Navigate to the correct category
                ButtonBase categoryButton = (ButtonBase)cashierView.FindName(category);

                var categoryCommand = categoryButton.Command;
                var categoryParam = categoryButton.CommandParameter;

                categoryCommand.Execute(categoryParam);

                foreach (string[] orderProducts in orderItems[category])
                {
                    // If it's a pizza we select the size menu from the second element in the array
                    if (category == "Pizza")
                    {
                        // Navigate to the correct size menu for pizzas
                        ButtonBase sizeButton = (ButtonBase)cashierView.FindName(orderProducts[1]);

                        var sizeCommand = sizeButton.Command;  
                        var sizeParam = sizeButton.CommandParameter;

                        sizeCommand.Execute(sizeParam);
                    }
                    
                    // Add the product to the order
                    Product productToAdd = _cashierViewModel.CurrentProductMenu.First(p => p.MenuName ==  orderProducts[0]);
                    productToAdd.AddOrderItemCommand.Execute(productToAdd);
                }
            }

            string orderTotal = _cashierViewModel.OrderTotal;

            // Now that our order is populated we can continue with checkout
            ButtonBase checkoutButton = cashierView.Checkout;

            var checkoutCommand = checkoutButton.Command;
            var checkoutParam = checkoutButton.CommandParameter;

            checkoutCommand.Execute(checkoutParam);

            Assert.That(_mainViewModel.CurrentViewModel,
                        Is.TypeOf<CheckoutViewModel>(),
                        "Current View Model should change to CheckoutViewModel");

            // Wait for the CashierView to render
            UpdateUI();

            // Now we can check that the checkout view renders as expected            
            var checkoutView = window.FindDescendant<PizzaShed.Views.Pages.CheckoutView>();
            Assert.That(checkoutView,
                        Is.Not.Null,
                        "Checkout view should have been rendered to the main window");

            _checkoutViewModel = (CheckoutViewModel)checkoutView.DataContext;

            // Now we check to make sure out checkout view behaves as expected
            Assert.That(_checkoutViewModel.TotalPriceValue, 
                        Is.EqualTo(orderTotal),
                        "Total price should be the same as the Cashier View");
            Assert.That(_checkoutViewModel.Promotions.Count,
                        Is.EqualTo(promoCount),
                        "The checkout viewmodel should only contain promotions that are applicable to the order");
            Assert.That(_checkoutViewModel.OrderProducts.Count,
                        Is.EqualTo(itemCount),
                        "OrderProducts should contain the same number of items as the order");

            // Now we check to make sure the UI behaves as expected
            Assert.That(checkoutView.OrderReceipt.Items.Count,
                        Is.EqualTo(itemCount),
                        "Order Receipt should contain the same number of items as the order");
            Assert.That(checkoutView.Promotions.Visibility,
                        Is.EqualTo(promoCount > 0 ? Visibility.Visible : Visibility.Collapsed),
                        "Promotion drop down should only be visible when there are promotions available");
        }
    }
}
