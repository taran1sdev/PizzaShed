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
using System.Printing;
using PizzaShed.Views.Windows;

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

        // Might not be the best way to store test data but it creates a clear map of how the 
        // user would interact with the UI
        private static Dictionary<int, Dictionary<string, string[][]>> _testOrders = new() {
            {0, 
                new()
                {
                    {"Pizza", [["Margherita", "Small"],["Pepperoni", "Medium"]] },
                    {"Kebab", [["Chicken Shish\n(Regular)"]]},
                    {"Drink", [["Coke\n(330ml)"]] },
                    {"Side", [["Garlic Mayo\n(60ml)"], ["Chicken Wings\n(6)"]] }

            } },
            {1, 
                new()
                {
                    {"Pizza", [["Pepperoni", "Large"]] }
                }                            
            },
            {2,
                new()
                {
                    {"Burger", [["Cheese Burger"]]}
                } 
            },
            {3,
                new()
                {
                    {"Deal", [["Margherita, Chips & Drink"], ["Coke\n(330ml)"]]}
                }
            },
            {4,
                new()
                {
                     {"Deal", [["Family Deal"], ["Margherita"], ["Doner Delight"], ["7UP\n(1.25l)"]]}
                }
            },
            {5,
                new()
                {
                    {"Deal", [["Family Deal"], ["Margherita"], ["Doner Delight"], ["7UP\n(1.25l)"]]},
                        {"Pizza", [["Margherita", "Small"],["Pepperoni", "Medium"]] },
                        {"Kebab", [["Chicken Shish\n(Regular)"]]},
                        {"Drink", [["Coke\n(330ml)"]] },
                        {"Side", [["Garlic Mayo\n(60ml)"], ["Chicken Wings\n(6)"]] }
                }
            }
        };

        private static IEnumerable<TestCaseData> OrderCreationTestCases
        {
            get
            {                
                yield return new TestCaseData(
                    _testOrders[0], 2, 6, "£6.37"
                ).SetName("OrderCreation_NavigatesToCheckout_WithCorrectData_LargeOrder_Promo_Both");
                yield return new TestCaseData(
                    _testOrders[1], 1, 1, "£2.60"
                ).SetName("OrderCreation_NavigatesToCheckout_WithCorrectData_SmallOrder_Promo_One");
                yield return new TestCaseData(
                    _testOrders[2], 1, 1, "£1.10"
                ).SetName("OrderCreation_NavigatesToCheckout_WithCorrectData_SmallOrder_Promo_None");
                yield return new TestCaseData(
                    _testOrders[3], 1, 4, "£2.20"
                ).SetName("OrderCreation_NavigatesToCheckout_WithCorrectData_SmallDealOrder_Promo_None");
                yield return new TestCaseData(
                    _testOrders[4], 1, 5, "£5.00"
                ).SetName("OrderCreation_NavigatesToCheckout_WithCorrectData_LargeDealOrder_Promo_None");
                yield return new TestCaseData(
                    _testOrders[5], 2, 11, "£11.37"
                ).SetName("OrderCreation_NavigatesToCheckout_WithCorrectData_LargeMixedOrder_Promo_Both");
            }
        }        

        // We already know from Unit tests that Order total is calculated correctly so we can just confirm that checkout displays the same
        [TestCaseSource(nameof(OrderCreationTestCases))]
        public void OrderCreation_NavigatesToCheckout_WithCorrectData(
            Dictionary<string, string[][]> orderItems, 
            int promoCount, 
            int itemCount,
            string expectedVAT)
        {
            var window = new PizzaShed.Views.Windows.MainWindow
            {
                DataContext = _mainViewModel
            };
            window.Show();

            UpdateUI();

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
            Assert.That(checkoutView.DiscountLabel.Visibility,
                        Is.EqualTo(promoCount > 0 ? Visibility.Visible : Visibility.Collapsed),
                        "Discount label should be visible when there are valid promotions");
            Assert.That(checkoutView.DiscountValue.Visibility,
                        Is.EqualTo(promoCount > 0 ? Visibility.Visible : Visibility.Collapsed),
                        "Discount value should be visible when there are valid promotions");
            Assert.That(checkoutView.VATValue.Text,
                        Is.EqualTo(expectedVAT),
                        "VAT Value should match 20% of the order cost");

            window.Hide();
        }

        private static Dictionary<string, Dictionary<string, decimal>> _orderPayments = new()
        {
            // Payments for our first order
            {"Zero - Cash",
                new()
                {
                    { "Cash", 31.86m }
                }
            },
            {"Zero - Card",
                new()
                {
                    { "Card", 31.86m }
                }
            },
            {"Zero - Split",
                new()
                {
                    { "Card", 11.86m },
                    { "Cash", 20.00m }
                }
            },
            {"Zero - Discount",
                new()
                {
                    { "Cash", 28.67m }
                }
            },
            {"One - Cash",
                new()
                {
                    { "Cash", 12.99m }
                }
            },
            {"One - Card",
                new()
                {
                    { "Card", 12.99m }
                }
            },
            {"One - Discount",
                new()
                {
                    { "Cash", 11.69m }
                }
            },
            {"Two - Cash",
                new()
                {
                    {"Cash", 5.49m }
                }
            },
            {"Two - Card",
                new()
                {
                    {"Card", 5.49m }
                }
            },
            {"Three - Split Change",
                new()
                {
                    {"Card", 10m },
                    {"Cash", 10m }
                }
            },
            {"Four - Cash Change",
                new()
                {
                    {"Cash", 25m }
                }
            },
            {"Five - Split Change",
                new()
                {
                    {"Card", 50m },
                    {"Cash", 10m }
                }
            }            
        };

        // We create payment scenarios for the previous orders 
        private static IEnumerable<TestCaseData> CounterPaymentTestCases
        {
            get
            {
                yield return new TestCaseData(
                    _testOrders[0],                    
                    false,
                    _orderPayments["Zero - Cash"],
                    "£0.00")
                    .SetName("Checkout_HandlesCounterPayments_AndNavigates_ToCashierView_Cash_NoPromo_Zero");
                yield return new TestCaseData(
                    _testOrders[0],                    
                    false,
                    _orderPayments["Zero - Card"],
                    "£0.00")
                    .SetName("Checkout_HandlesCounterPayments_AndNavigates_ToCashierView_Card_NoPromo_Zero");                
                yield return new TestCaseData(
                    _testOrders[0],                    
                    false,
                    _orderPayments["Zero - Split"],
                    "£0.00")
                    .SetName("Checkout_HandlesCounterPayments_AndNavigates_ToCashierView_Split_NoPromo_Zero");
                yield return new TestCaseData(
                    _testOrders[0],
                    true,
                    _orderPayments["Zero - Discount"],
                    "£0.00")
                    .SetName("Checkout_HandlesCounterPayments_AndNavigates_ToCashierView_Cash_Promo_Zero");
                yield return new TestCaseData(
                    _testOrders[1],
                    false,
                    _orderPayments["One - Cash"],
                    "£0.00")
                    .SetName("Checkout_HandlesCounterPayments_AndNavigates_ToCashierView_Cash_NoPromo_One");
                yield return new TestCaseData(
                    _testOrders[1],
                    false,
                    _orderPayments["One - Card"],
                    "£0.00")
                    .SetName("Checkout_HandlesCounterPayments_AndNavigates_ToCashierView_Card_NoPromo_One");
                yield return new TestCaseData(
                    _testOrders[1],
                    true,
                    _orderPayments["One - Discount"],
                    "£0.00")
                    .SetName("Checkout_HandlesCounterPayments_AndNavigates_ToCashierView_Cash_Promo_One");
                yield return new TestCaseData(
                   _testOrders[2],
                   false,
                   _orderPayments["Two - Cash"],
                   "£0.00")
                   .SetName("Checkout_HandlesCounterPayments_AndNavigates_ToCashierView_Cash_NoPromo_Two");
                yield return new TestCaseData(
                    _testOrders[2],
                    false,
                    _orderPayments["Two - Card"],
                    "£0.00")
                    .SetName("Checkout_HandlesCounterPayments_AndNavigates_ToCashierView_Card_NoPromo_Two");
                yield return new TestCaseData(
                    _testOrders[3],
                    false,
                    _orderPayments["Three - Split Change"],
                    "£-9.01")
                    .SetName("Checkout_HandlesCounterPayments_AndNavigates_ToCashierView_Card_NoPromo_Three_Change");
                yield return new TestCaseData(
                    _testOrders[4],
                    false,
                    _orderPayments["Four - Cash Change"],
                    "£-0.01")
                    .SetName("Checkout_HandlesCounterPayments_AndNavigates_ToCashierView_Cash_NoPromo_Four_Change");
                yield return new TestCaseData(
                    _testOrders[5],
                    false,
                    _orderPayments["Five - Split Change"],
                    "£-3.15")
                    .SetName("Checkout_HandlesCounterPayments_AndNavigates_ToCashierView_Split_NoPromo_Five_Change");
                yield return new TestCaseData(
                    _testOrders[5],
                    true,
                    _orderPayments["Five - Split Change"],
                    "£-8.84")
                    .SetName("Checkout_HandlesCounterPayments_AndNavigates_ToCashierView_Split_Promo_Five_Change");
            }
        }


        // Ensure that payments work as expected - cash, card, card n. present, split - and change is displayed on overpayment 
        [TestCaseSource(nameof(CounterPaymentTestCases))]
        public void Checkout_HandlesCounterPayments_AndNavigates_ToCashierView(
            Dictionary<string, string[][]> orderItems,            
            bool isPromo,
            Dictionary<string, decimal> orderPayments,
            string expectedTotal)
        {
            var window = new PizzaShed.Views.Windows.MainWindow
            {
                DataContext = _mainViewModel
            };
            window.Show();

            UpdateUI();

            // We populate the orders like before 
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
                    Product productToAdd = _cashierViewModel.CurrentProductMenu.First(p => p.MenuName == orderProducts[0]);
                    productToAdd.AddOrderItemCommand.Execute(productToAdd);
                }
            }            

            // Now that our order is populated we can continue with checkout
            ButtonBase checkoutButton = cashierView.Checkout;

            var checkoutCommand = checkoutButton.Command;
            var checkoutParam = checkoutButton.CommandParameter;

            checkoutCommand.Execute(checkoutParam);

            UpdateUI();

            var checkoutView = window.FindDescendant<PizzaShed.Views.Pages.CheckoutView>();
            Assert.That(checkoutView, Is.Not.Null);

            _checkoutViewModel = (CheckoutViewModel)checkoutView.DataContext;           

            if (isPromo)
            {
                // If we are testing a promotion assign it in the view-model
                _checkoutViewModel.SelectedPromotion = _checkoutViewModel.Promotions.First();
            }

            foreach (string key in orderPayments.Keys)
            {
                ButtonBase paymentMethodButton = (ButtonBase)checkoutView.FindName(key);
                var paymentMethodButtonCommand = paymentMethodButton.Command;
                var paymentMethodButtonParam = paymentMethodButton.CommandParameter;

                paymentMethodButtonCommand.Execute(paymentMethodButtonParam);

                UpdateUI();

                var paymentWindow = Application.Current.Windows.OfType<PaymentWindow>().First(w => w.IsVisible);
                Assert.That(
                    paymentWindow,
                    Is.Not.Null,
                    "Payment Window should be the correct type");

                var paymentView = paymentWindow.FindDescendant<PizzaShed.Views.Pages.PaymentPresentView>();
                Assert.That(
                    paymentView,
                    Is.Not.Null,
                    "Payment view should not be null");

                PaymentPresentViewModel paymentViewModel = (PaymentPresentViewModel)_mainViewModel.PaymentViewModel;

                Assert.That(
                    paymentViewModel,
                    Is.TypeOf<PaymentPresentViewModel>(),
                    "Payment Windows view model should be the correct type");

                // This should set the total to our payment amount
                paymentViewModel.Total = $"{orderPayments[key]:N2}";

                ButtonBase paymentButton = (ButtonBase)paymentView.Submit;
                var paymentCommand = paymentButton.Command;
                var paymentParam = paymentButton.CommandParameter;

                // This should make the payment and hide the window
                paymentCommand.Execute(paymentParam);
                paymentWindow.Hide();
                UpdateUI();
            }
                        
            _checkoutViewModel = (CheckoutViewModel)checkoutView.DataContext;

            UpdateUI();            

            // Ensure the checkout view model behaves as expected
            Assert.That(
                _checkoutViewModel.IsPaid,
                Is.True,
                "Order should now be marked as paid");
            Assert.That(
                _checkoutViewModel.TotalPriceValue,
                Is.EqualTo(expectedTotal),
                "Order Total should be calculated after payments");            

            //Ensure the UI behaves as expected
            Assert.That(
                checkoutView.Card.Visibility,
                Is.EqualTo(Visibility.Collapsed),
                "Card payment button should no longer be visible");
            Assert.That(
                checkoutView.Complete.Visibility,
                Is.EqualTo(Visibility.Visible),
                "Complete button should now be visible");
            Assert.That(
                checkoutView.OrderTotalValue.Text,
                Is.EqualTo(expectedTotal),
                "UI should display the correct total after payments");

            // We can complete the order and make sure we navigate back to checkout view
            ButtonBase completeButton = checkoutView.Complete;
            var completeCommand = completeButton.Command;
            var completeParam = completeButton.CommandParameter;

            completeCommand.Execute(completeParam);

            UpdateUI();

            Assert.That(
                _mainViewModel.CurrentViewModel,
                Is.TypeOf<CashierViewModel>(),
                "After order completion we should navigate back to the cashier view");

            window.Hide();
        }
    }
}
