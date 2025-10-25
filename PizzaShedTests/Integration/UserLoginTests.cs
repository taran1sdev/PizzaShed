using System;
using System.Threading;
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
using NUnit.Framework;
using PizzaShed;
using PizzaShed.Model;
using PizzaShed.Services.Data;
using PizzaShed.ViewModels;
using PizzaShed.Views.Windows;

namespace PizzaShedTests.Integration
{
    [Apartment(ApartmentState.STA)]
    [TestFixture]
    public class UserLoginTests
    {
        private readonly IDatabaseManager _databaseManager = DatabaseManager.Instance;
        
        // We create our real objects needed to test the login flow
        private IUserRepository _userRepository;
        private Session _session;

        private MainViewModel _mainViewModel;
        private LoginViewModel _loginViewModel;
        
        // We mock our other required dependencies that are not required to test login flow
        private Mock<IProductRepository<Product>> _productRepository;
        private Mock<IProductRepository<Topping>> _toppingRepository;
        private Mock<IOrderRepository> _orderRepository;
        private Mock<ICustomerRepository> _customerRepository;

        private static Dictionary<string, User> _testUsers = new Dictionary<string, User>
        {
            {
                "Manager",
                new User(1, "Test", "Manager")
            },
            {
                "Cashier",
                new User(2, "Test", "Cashier")
            },
            {
                "Pizzaiolo",
                new User(3, "Test", "Pizzaiolo")
            },
            {
                "Grill Cook",
                new User(4, "Test", "Grill Cook")
            },
            {
                "Driver",
                new User(5, "Test", "Driver")
            }
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

        [OneTimeTearDown]
        public void FixtureTearDown()
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                if (Application.Current is { }) Application.Current.Shutdown();
            });
        }


        [SetUp]
        public void SetUp()
        {
            _session = new Session();           
            
            _userRepository = new UserRepository(_databaseManager);            

            _productRepository = new Mock<IProductRepository<Product>>();
            _toppingRepository = new Mock<IProductRepository<Topping>>();
            _orderRepository = new Mock<IOrderRepository>();
            _customerRepository = new Mock<ICustomerRepository>();

            // We set up our mocks to return empty lists so we avoid null exceptions 
            _productRepository.Setup(p => p.GetMealDeals()).Returns([]);
            _orderRepository.Setup(o => o.GetCollectionOrders()).Returns([]);
            _orderRepository.Setup(o => o.GetKitchenOrders(It.IsAny<bool>())).Returns([]);
            _orderRepository.Setup(o => o.GetDeliveryOrders()).Returns([]);

            _mainViewModel = new MainViewModel(
                                    _session, 
                                    _userRepository, 
                                    _productRepository.Object, 
                                    _toppingRepository.Object, 
                                    _orderRepository.Object, 
                                    _customerRepository.Object);

            Assert.That(_mainViewModel.CurrentViewModel, Is.TypeOf<LoginViewModel>());

            _loginViewModel = (LoginViewModel)_mainViewModel.CurrentViewModel;
        }

        private static IEnumerable<TestCaseData> UserLoginTestCases
        {
            get
            {
                yield return new TestCaseData("Manager", "0000", typeof(CashierViewModel))
                    .SetName("UserLoginManager");
                yield return new TestCaseData("Cashier", "0001", typeof(CashierViewModel))
                    .SetName("UserLoginCashier");
                yield return new TestCaseData("Pizzaiolo", "0002", typeof(OrderViewModel))
                    .SetName("UserLoginPizzaiolo");
                yield return new TestCaseData("Grill Cook", "0003", typeof(OrderViewModel))
                    .SetName("UserLoginGrillCook");
                yield return new TestCaseData("Driver", "0004", typeof(OrderViewModel))
                    .SetName("UserLoginDriver");
            }
        }

        
        [TestCaseSource(nameof(UserLoginTestCases))]
        public void UserRepsoitory_ReturnsCorrect_UserObject_And_NavigateToView(string expectedRole, string pin, Type expectedViewModelType)
        {                                    
            // Enter the user's pin
            _loginViewModel.Pin = pin;                        

            // Make sure the session is updated with the correct user
            Assert.That(_session.IsLoggedIn, Is.True, "Session IsLoggedIn should return true");
            Assert.That(_session.UserRole, Is.EqualTo(expectedRole), "User role should match test data");
            
            Assert.That(_mainViewModel.CurrentViewModel, Is.TypeOf(expectedViewModelType), "Application should navigate to the correct view");
        }

        private static IEnumerable<TestCaseData> UserLogoutTestCases
        {
            get
            {
                yield return new TestCaseData("Manager")
                .SetName("UserLogout_RedirectsToLoginView_Manager");
                yield return new TestCaseData("Cashier")
                    .SetName("UserLogout_RedirectsToLoginView_Cashier");
                yield return new TestCaseData("Pizzaiolo")
                    .SetName("UserLogout_RedirectsToLoginView_Pizzaiolo");
                yield return new TestCaseData("Grill Cook")
                    .SetName("UserLogout_RedirectsToLoginView_GrillCook");
                yield return new TestCaseData("Driver")
                    .SetName("UserLogout_RedirectsToLoginView_Driver");
            }            
        }

        [Test]
        public void InvalidPin_StaysOnLogin()
        {
            _loginViewModel.Pin = "9999";

            
            // Ensure the session is not updated and the application remains on the login view
            Assert.That(_session.IsLoggedIn, Is.False, "Session should not be updated on a failed login");
            Assert.That(_mainViewModel.CurrentViewModel, Is.TypeOf<LoginViewModel>(), "Application should not navigate after failed login");
        }

        [TestCaseSource(nameof(UserLogoutTestCases))]
        public void UserLogout_RedirectsToLoginView(string role)
        {
            // We login using the test user data - this should call the SessionChanged event and redirect
            _session.Login(_testUsers[role]);

            // Ensure the session updates correctly when login function is called
            Assert.That(_session.IsLoggedIn, Is.True, "Session login function should log in the user");
            Assert.That(_mainViewModel.CurrentViewModel, Is.Not.TypeOf<LoginViewModel>(), "Application should navigate to correct view after login");

            _session.Logout();

            // Make sure the session and current view is updated on logout            
            Assert.That(_session.IsLoggedIn, Is.False, "Logout should update the session object");

            Assert.That(_mainViewModel.CurrentViewModel, Is.TypeOf<LoginViewModel>(), "Logging out should navigate to Login View");
        }
    }
}