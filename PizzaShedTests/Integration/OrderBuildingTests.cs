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
    public class OrderBuildingTests
    {
        private IDatabaseManager _databaseManager = DatabaseManager.Instance;
        
        private IUserRepository _userRepository;
        private ISession _session;
        private MainViewModel _mainViewModel;
        private CashierViewModel _cashierViewModel;
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
            else
            {                               
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
        }

        [Test]
        public void CashierView_LoadsCorrectly()
        {
            // We create our applications window
            var window = new PizzaShed.Views.Windows.MainWindow
            {
                DataContext = _mainViewModel
            };
            window.Show();

            UpdateUI();

            // We use our helper function from behaviours to find the CashierView in the Window
            var cashierView = window.FindDescendant<PizzaShed.Views.Pages.CashierView>();

            // Ensure the View is loaded as expected
            Assert.That(cashierView, Is.Not.Null, "Window should load the cashier view after login");

            Assert.That(cashierView.DataContext, Is.TypeOf<CashierViewModel>(), "CashierViewModel should be set correctly");
            window.Hide();
        }

        [TestCase("Deal", null, false)]        
        [TestCase("Kebab", null, true)]
        [TestCase("Burger", "Wrap", false)]
        [TestCase("Side", "Dip", false)]
        [TestCase("Drink", null, false)]
        public void MenuCategory_RendersObjectsCorrectly(string category, string? secondaryCategory, bool showToppings)
        {
            // We create our applications windowa
            var window = new PizzaShed.Views.Windows.MainWindow
            {
                DataContext = _mainViewModel
            };
            window.Show();

            UpdateUI();

            // We use our helper function from behaviours to find the CashierView in the Window
            var cashierView = window.FindDescendant<PizzaShed.Views.Pages.CashierView>();

            // We get the view model from the window
            Assert.That(cashierView, Is.Not.Null);
            _cashierViewModel = (CashierViewModel)cashierView.DataContext;

            // Find the button to be tested in the view
            ButtonBase categoryButton = (ButtonBase)cashierView.FindName(category);           
            Assert.That(categoryButton, Is.Not.Null, "Category button should be set");            

            // We execute the buttons command directly - sending a click event doesn't work as expected
            var command = categoryButton.Command;
            var param = categoryButton.CommandParameter;

            command.Execute(param);
            
            // Check the View Model Properties are set as expected
            Assert.That(
                _cashierViewModel.SelectedCategory, 
                Is.EqualTo(category), 
                "Category button click should change category");
            Assert.That(
                _cashierViewModel.DisplayToppings, 
                Is.EqualTo(showToppings), 
                "Toppings should only be visible for certain categories");
            if (secondaryCategory != null)
            {
                Assert.That(
                    _cashierViewModel
                        .CurrentProductMenu
                        .All(p => 
                            p.Category == category 
                            || p.Category == secondaryCategory),
                    Is.True,
                    "Product menu should only contain items from selected category");
            } 
            else
            {
                Assert.That(
                _cashierViewModel
                    .CurrentProductMenu
                        .All(p => p.Category == category),
                Is.True,
                "Product menu should only contain items from selected category");
            }



                // Check that the UI reacts as expected
            ItemsControl productMenu = (ItemsControl)cashierView.FindName("ProductMenu");
            ItemsControl toppingMenu = (ItemsControl)cashierView.FindName("ToppingMenu");

            if (secondaryCategory != null)
            {
                Assert.That(
                    productMenu
                        .Items
                        .OfType<Product>()
                        .All(p =>
                            p.Category == category
                            || p.Category == secondaryCategory),
                    Is.True,
                    "UI should only display contain items from selected category");
            }
            else
            {
                Assert.That(
                productMenu
                    .Items
                    .OfType<Product>()
                    .All(p => p.Category == category),
                Is.True,
                "UI menu should only contain items from selected category");
            }
            Assert.That(
                toppingMenu.Visibility, 
                Is.EqualTo(showToppings ? Visibility.Visible : Visibility.Collapsed), 
                "ToppingMenu should only be visible for certain categories");
            window.Hide();
        }

        [TestCase("Pizza", "Small")]
        [TestCase("Pizza", "Medium")]
        [TestCase("Pizza", "Large")]        
        public void PizzaCategory_RendersObjectsCorrectly(string category, string size)
        {
            // We create our applications window
            var window = new PizzaShed.Views.Windows.MainWindow
            {
                DataContext = _mainViewModel
            };
            window.Show();

            UpdateUI();

            // We use our helper function from behaviours to find the CashierView in the Window
            var cashierView = window.FindDescendant<PizzaShed.Views.Pages.CashierView>();

            // We get the view model from the window
            Assert.That(cashierView, Is.Not.Null);
            _cashierViewModel = (CashierViewModel)cashierView.DataContext;

            // Find the button to be tested in the view
            ButtonBase categoryButton = (ButtonBase)cashierView.FindName(category);
            Assert.That(categoryButton, Is.Not.Null, "Category button should be set");

            // We execute the buttons command directly - sending a click event doesn't work as expected
            var command = categoryButton.Command;
            var param = categoryButton.CommandParameter;

            command.Execute(param);

            // Find the button to be tested in the view
            ButtonBase sizeButton = (ButtonBase)cashierView.FindName(size);
            Assert.That(sizeButton, Is.Not.Null, "size button should be set");
            Assert.That(sizeButton.Visibility, Is.EqualTo(Visibility.Visible), "Size button should be visible");

            // We execute the buttons command directly - sending a click event doesn't work as expected
            var sizeCommand = sizeButton.Command;
            var sizeParam = sizeButton.CommandParameter;

            sizeCommand.Execute(sizeParam);

            // Check the View Model Properties are set as expected
            Assert.That(_cashierViewModel.SelectedCategory, Is.EqualTo(category), "Category button click should change category");
            Assert.That(_cashierViewModel.CurrentSizeSelection, Is.EqualTo(size), "Category button click should change category");
            Assert.That(_cashierViewModel.DisplayToppings, Is.EqualTo(true), "Toppings should only be visible for certain categories");

            Assert.That(_cashierViewModel.CurrentProductMenu.Select(p => p.Category == category), Is.Not.Null, "Product menu should contain items from selected category");
            Assert.That(_cashierViewModel.CurrentProductMenu.All(p => p.SizeName == size), "Product menu should only contain pizzas with the current size selection");

            Assert.That(_cashierViewModel.CurrentToppingMenu.First(t => t.ToppingType == "Meat"), Is.Not.Null, "Topping menu should include meat toppings");
            Assert.That(_cashierViewModel.CurrentToppingMenu.First(t => t.ToppingType == "Veg"), Is.Not.Null, "Topping menu should include Veg toppings");
            Assert.That(_cashierViewModel.CurrentToppingMenu.Where(t => t.ChoiceRequired).Count, Is.EqualTo(1), "Topping menu should include one Base topping");

            // Check that the UI reacts as expected
            ItemsControl productMenu = (ItemsControl)cashierView.FindName("ProductMenu");
            ItemsControl toppingMenu = (ItemsControl)cashierView.FindName("ToppingMenu");

            Assert.That(productMenu.Items, Is.Not.Null, "ProductMenu should display items");
            Assert.That(toppingMenu.Visibility, Is.EqualTo(Visibility.Visible), "ToppingMenu should only be visible for certain categories");
            Assert.That(toppingMenu.Items, Is.Not.Null);
            window.Hide();
        }        

        private static IEnumerable<TestCaseData> ProductTestCases
        {
            get
            {
                yield return new TestCaseData("Margherita", "Pizza");
                yield return new TestCaseData("Chicken Shish\n(Regular)", "Kebab");
                yield return new TestCaseData("Chips\n(Regular)", "Side");
                yield return new TestCaseData("Cheese Burger", "Burger");
                yield return new TestCaseData("Coke\n(330ml)", "Drink");
            }
        }

        // Ensure that products are added to the current order list as expected
        [TestCaseSource(nameof(ProductTestCases))]
        public void ProductAddOrderItem_AddsProduct_ToCurrentOrderItems(string productName, string productCategory)
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

            // Navigate to the correct category 
            ButtonBase categoryButton = (ButtonBase)cashierView.FindName(productCategory);

            var categoryCmd = categoryButton.Command;
            var categoryParam = categoryButton.CommandParameter;

            categoryCmd.Execute(categoryParam);

            Product productToAdd = _cashierViewModel.CurrentProductMenu.First(p => p.MenuName == productName);

            // We add the product to the menu
            productToAdd.AddOrderItemCommand.Execute(productToAdd);

            // Check the view model behaves as expected
            Assert.That(_cashierViewModel.CurrentOrderItems.Count, 
                        Is.EqualTo(1), 
                        "Only one item should be added to the view model");
            Assert.That(_cashierViewModel.CurrentOrderItems.First().MenuName, 
                        Is.EqualTo(productName), 
                        "The product added in the view model should be the one selected in the view");
            Assert.That(_cashierViewModel.SelectedOrderItem, 
                        Is.Not.Null, 
                        "A product should now be selected in the view model");
            Assert.That(_cashierViewModel.SelectedOrderItem.MenuName, 
                        Is.EqualTo(productName), 
                        "The product selected in the view model should be the one selected in the view");

            // Check the order list in the view behaves as expected
            ListView orderList = (ListView)cashierView.FindName("OrderItems");

            Assert.That(orderList.Items.Count,
                        Is.EqualTo(1),
                        "There should only be one item in the order list");
            Assert.That(orderList.Items.GetItemAt(0),
                        Is.TypeOf<Product>(),
                        "Item in list view should be a product");
            window.Hide();
        }

        [TestCaseSource(nameof(ProductTestCases))]
        public void VoidButton_RemovesProduct_FromCurrentOrderItems(string productName, string productCategory)
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

            // Navigate to the correct category 
            ButtonBase categoryButton = (ButtonBase)cashierView.FindName(productCategory);

            var categoryCmd = categoryButton.Command;
            var categoryParam = categoryButton.CommandParameter;

            categoryCmd.Execute(categoryParam);

            Product productToAdd = _cashierViewModel.CurrentProductMenu.First(p => p.MenuName == productName);

            // We add the product to the menu
            productToAdd.AddOrderItemCommand.Execute(productToAdd);

            ButtonBase voidButton = (ButtonBase)cashierView.FindName("Void");

            var voidCmd = voidButton.Command;
            var voidParameter = voidButton.CommandParameter;
            voidCmd.Execute(voidParameter);

            // Check the view model behaves as expected
            Assert.That(_cashierViewModel.CurrentOrderItems.Count,
                        Is.EqualTo(0),
                        "There should be no items in the view model");            
            Assert.That(_cashierViewModel.SelectedOrderItem,
                        Is.Null,
                        "Selected Item should be null with no products");            

            // Check the order list in the view behaves as expected
            ListView orderList = (ListView)cashierView.FindName("OrderItems");

            Assert.That(orderList.Items.Count,
                        Is.EqualTo(0),
                        "There should only be one item in the order list");

            window.Hide();
        }

        public static IEnumerable<TestCaseData> DealTestCases
        {
            get
            {
                yield return new TestCaseData("Margherita, Chips & Drink", 4);
                yield return new TestCaseData("Large Pizza & Dips", 4);
                yield return new TestCaseData("Family Deal", 5);
                yield return new TestCaseData("Kebab Meal", 4);
            }
        }

        [TestCaseSource(nameof(DealTestCases))]
        public void ProductAddOrderItemCommand_DealItem_AddsDealItemsToCurrentOrder(string dealName, int itemCount)
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

            Product dealToTest = _cashierViewModel.CurrentProductMenu.First(p => p.MenuName == dealName);

            dealToTest.AddOrderItemCommand.Execute(dealToTest);

            // Check the view model behaves as expected
            Assert.That(_cashierViewModel.CurrentOrderItems.Count, 
                        Is.EqualTo(itemCount), 
                        "CurrentOrderItems count should be the same as the number of items in the deal + 1");
            Assert.That(_cashierViewModel.CurrentOrderItems.All(p => p.ParentDealID != null),
                        Is.True,
                        "All Items should have a parent deal ID");
            Assert.That(_cashierViewModel.SelectedOrderItem,
                        Is.Not.Null,
                        "An order item should be selected");
            Assert.That(_cashierViewModel.SelectedOrderItem.IsPlaceholder,
                        Is.True,
                        "The selected item should be a placeholder item");
            Assert.That(_cashierViewModel.SelectedCategory,
                        Is.EqualTo(_cashierViewModel.SelectedOrderItem.Category),
                        "The current category should be the same as the first deal item that requires selection");

            // Check the UI behaves as expected
            ListView orderItems = (ListView)cashierView.OrderItems;

            Assert.That(orderItems.Items.Count,
                        Is.EqualTo(itemCount),
                        "The UI should display the same number of items in the deal + the deal item");
            window.Close();
        }

        [TestCaseSource(nameof(DealTestCases))]
        public void VoidButton_DealItem_RemovesAllItemsInDeal(string dealName, int itemCount)
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

            Product dealToTest = _cashierViewModel.CurrentProductMenu.First(p => p.MenuName == dealName);

            ButtonBase voidButton = (ButtonBase)cashierView.Void;

            var voidCommand = voidButton.Command;
            var voidParam = voidButton.CommandParameter;

            for (int i = 0; i < itemCount; i++)
            {
                dealToTest.AddOrderItemCommand.Execute(dealToTest);

                _cashierViewModel.SelectedOrderItem = _cashierViewModel.CurrentOrderItems[i];

                voidCommand.Execute(voidParam);

                // Check the view model behaves as expected
                Assert.That(_cashierViewModel.CurrentOrderItems.Count,
                            Is.EqualTo(0),
                            "CurrentOrderItems count should 0");
                Assert.That(_cashierViewModel.SelectedOrderItem,
                            Is.Null,
                            "Selected Order Item should be null with no order items");
                Assert.That(_cashierViewModel.SelectedCategory,
                            Is.EqualTo("Deal"),
                            "The current category should be Deal");

                // Check the UI behaves as expected
                ListView orderItems = (ListView)cashierView.OrderItems;

                Assert.That(orderItems.Items.Count,
                            Is.EqualTo(0),
                            "The UI should display no items");
            }
            
            window.Close();
        }
    }
}