using Moq;
using Moq.Protected;
using NUnit.Framework;
using PizzaShed.Model;
using PizzaShed.Services.Data;
using PizzaShed.ViewModels;
using PizzaShed.Views.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PizzaShedTests.Unit
{
    [TestFixture]
    public  class CustomerViewModelTests
    {
        private Mock<ICustomerRepository> _customerRepository;
        private Mock<IOrderRepository> _orderRepository;
        private static int testOrderID = 1;
        private CustomerViewModel _customerViewModel;
        private Customer testCustomer;
        private Customer newCustomer;

        [SetUp]
        public void SetUp()
        {
            _customerRepository = new Mock<ICustomerRepository>();
            _orderRepository = new Mock<IOrderRepository>();

            testCustomer = new Customer
            {
                ID = 1,
                Name = "Test Customer",
                StreetAddress = "Sesame Street",
                House = "10",
                PhoneNumber = "01234567890",
                Postcode = "TA6 5NN",
                DeliveryNotes = "Some Note"
            };

            newCustomer = new Customer
            {
                ID = 0,
                Name = "Jane Doe",
                StreetAddress = "Some Address",
                House = "10",
                Postcode = "TA6 5NN",
                PhoneNumber = "01234567890"
            };

            _customerRepository.Setup(c => c.GetCustomerByPartialName("Test"))
                               .Returns(new ObservableCollection<Customer> { testCustomer });

            _customerViewModel = new CustomerViewModel(_orderRepository.Object, _customerRepository.Object, testOrderID);
        }

        // Ensure our properties are being initialized as expected
        [Test]
        public void Constructor_Initializes_Properties()
        {
            Assert.That(_customerViewModel.OrderID, Is.EqualTo(testOrderID));
            Assert.That(_customerViewModel.CurrentCustomer, Is.Not.Null);
        }

        // Ensure that our search customer search functionality works as expected
        [Test]
        public void SearchByPartialName_ReturnsCustomer()
        {
            _customerViewModel.NameSearch = "Test";

            _customerRepository.Verify(c => c.GetCustomerByPartialName("Test"), Times.Once,
                                        "GetCustomerByPartialName should be called once");

            Assert.That(_customerViewModel.CustomerSuggestion.Count, Is.EqualTo(1),
                        "CustomerSuggestion should hold our returned customer");
        }

        [Test]
        public void SelectingSuggestedCustomer_AutofillsProperties()
        {            
            _customerViewModel.NameSearch = "Test";

            _customerViewModel.ProxyCustomer = _customerViewModel.CustomerSuggestion.First();

            Assert.That(_customerViewModel.CurrentCustomer, Is.EqualTo(testCustomer),
                        "CurrentCustomer should contain the suggested customer after selection");
        }

        [Test]
        public void ClearCommand_Resets_Form()
        {
            _customerViewModel.NameSearch = "Test";

            _customerViewModel.ClearCommand.Execute(null);

            Assert.That(_customerViewModel.CurrentCustomer.ID, Is.EqualTo(0),
                        "CurrentCustomer ID should be reset to 0");

            Assert.That(_customerViewModel.NameSearch, Is.EqualTo(""),
                        "Name search Property should be empty on Clear");

            Assert.That(_customerViewModel.CurrentCustomer, Is.Not.EqualTo(testCustomer),
                        "CurrentCustomer should not be equal to previously selected customer");
        }


        public static IEnumerable<TestCaseData> CheckoutCommandFailureTestCases
        {
            get
            {
                yield return new TestCaseData(
                    new Customer
                    {
                        ID = 2,
                        Name = "", // Missing name
                        StreetAddress = "Some Street",
                        House = "10",
                        Postcode = "TA6 5NN",
                        PhoneNumber = "01234567890"
                    }).SetName("CheckoutCommand_FailsWhen_Name_Missing");
                yield return new TestCaseData(
                    new Customer
                    {
                        ID = 3,
                        Name = "Jane Doe",
                        StreetAddress = "",
                        House = "10",
                        Postcode = "TA6 5NN",
                        PhoneNumber = "01234567890"
                    }).SetName("CheckoutCommand_FailsWhen_StreetAddress_Missing");
                yield return new TestCaseData(
                    new Customer
                    {
                        ID = 4,
                        Name = "Jane Doe",
                        StreetAddress = "Some Street",
                        House = "",
                        Postcode = "TA6 5NN",
                        PhoneNumber = "01234567890"
                    }).SetName("CheckoutCommand_FailsWhen_House_Missing");
                yield return new TestCaseData(
                    new Customer
                    {
                        ID = 5,
                        Name = "Jane Dow",
                        StreetAddress = "Some Street",
                        House = "10",
                        Postcode = "",
                        PhoneNumber = "01234567890"
                    }).SetName("CheckoutCommand_FailsWhen_Postcode_Missing");
                yield return new TestCaseData(
                    new Customer
                    {
                        ID = 6,
                        Name = "Jane Doe",
                        StreetAddress = "Some Street",
                        House = "10",
                        Postcode = "TA6 5NN",
                        PhoneNumber = ""
                    }).SetName("CheckoutCommand_FailsWhen_PhoneNumber_Missing");
            }
        }

        // Ensure that CheckoutCommand fails when Required fields are missing
        [Test]
        [TestCaseSource(nameof(CheckoutCommandFailureTestCases))]
        public void CheckoutCommand_FailsWhen_RequiredFieldsMissing(Customer customer)
        {
            _customerViewModel.CurrentCustomer = customer;

            _customerViewModel.CheckoutCommand.Execute(null);

            _customerRepository.Verify(c => c.UpdateCustomer(customer), Times.Never,
                                        "Update Customer should not be called with required fields missing");
            _customerRepository.Verify(c => c.CreateNewCustomer(customer), Times.Never,
                                        "CreateNewCustomer should not be called with ID > 0 or missing fields");
        }


        // Ensure that CheckoutCommand succeeds when Required fields are present
        [Test]
        public void CheckoutCommand_SucceedsWhen_RequiredFieldsPresent_ExistingCustomer()
        {
            _customerViewModel.CurrentCustomer = testCustomer;

            _customerViewModel.CheckoutCommand.Execute(null);

            _customerRepository.Verify(c => c.UpdateCustomer(testCustomer), Times.Once,
                                        "Update Customer should be called on checkout with existing customer");
            _customerRepository.Verify(c => c.CreateNewCustomer(testCustomer), Times.Never,
                                        "CreateNewCustomer should not be called for an existing customer");
        }

        // Ensure that CheckoutCommand creates a new customer when Required fields are present
        [Test]
        public void CheckoutCommand_SucceedsWhen_RequiredFieldsPresent_NewCustomer()
        {
            _customerViewModel.CurrentCustomer = newCustomer;

            _customerViewModel.CheckoutCommand.Execute(null);

            _customerRepository.Verify(c => c.CreateNewCustomer(newCustomer), Times.Once,
                                        "CreateNewCustomer should be called when ID is 0");
            _customerRepository.Verify(c => c.UpdateCustomer(newCustomer), Times.Never,
                                        "Update customer should not be called on a new customer");
        }

        [Test]
        public void ErrorDisplayed_WhenUpdateCustomer_Fails()
        {
            _customerRepository.Setup(c => c.UpdateCustomer(testCustomer))
                               .Returns(false);

            _customerViewModel.CurrentCustomer = testCustomer;

            _customerViewModel.CheckoutCommand.Execute(null);

            Assert.That(_customerViewModel.ErrorMessage, Is.EqualTo("Failed to update\n customer record"),
                        "ErrorMessage should be displayed when customer update fails");
        }

        [Test]
        public void ErrorDisplayed_WhenCreateNewCustomer_Fails()
        {
            _customerRepository.Setup(c => c.CreateNewCustomer(newCustomer))
                               .Returns(0);

            _customerViewModel.CurrentCustomer = newCustomer;

            _customerViewModel.CheckoutCommand.Execute(null);

            Assert.That(_customerViewModel.ErrorMessage, Is.EqualTo("Failed to create\n new customer"),
                        "ErrorMessage should be displayed when customer creation fails");
        }

        public static IEnumerable<TestCaseData> CheckoutWithValidDistanceTestCases
        {
            get
            {
                yield return new TestCaseData(
                    new Customer
                    {
                        ID = 1,
                        Name = "Jane Doe",
                        StreetAddress = "Some Street",
                        House = "10",
                        Postcode = "TA6 5NN",
                        PhoneNumber = "01234567890"
                    }, 0).SetName("CheckoutSucceeds_WithValidDistance_0");
                yield return new TestCaseData(
                    new Customer
                    {
                        ID = 1,
                        Name = "Jane Doe",
                        StreetAddress = "Some Street",
                        House = "10",
                        Postcode = "TA6 4NN",
                        PhoneNumber = "01234567890"
                    }, 1).SetName("CheckoutSucceeds_WithValidDistance_1-");
                yield return new TestCaseData(
                    new Customer
                    {
                        ID = 1,
                        Name = "Jane Doe",
                        StreetAddress = "Some Street",
                        House = "10",
                        Postcode = "TA6 6NN",
                        PhoneNumber = "01234567890"
                    }, 1).SetName("CheckoutSucceeds_WithValidDistance_1+");
                yield return new TestCaseData(
                    new Customer
                    {
                        ID = 1,
                        Name = "Jane Doe",
                        StreetAddress = "Some Street",
                        House = "10",
                        Postcode = "TA6 3NN",
                        PhoneNumber = "01234567890"
                    }, 2).SetName("CheckoutSucceeds_WithValidDistance_2-");
                yield return new TestCaseData(
                    new Customer
                    {
                        ID = 1,
                        Name = "Jane Doe",
                        StreetAddress = "Some Street",
                        House = "10",
                        Postcode = "TA6 7NN",
                        PhoneNumber = "01234567890"
                    }, 2).SetName("CheckoutSucceeds_WithValidDistance_2+");
                yield return new TestCaseData(
                    new Customer
                    {
                        ID = 1,
                        Name = "Jane Doe",
                        StreetAddress = "Some Street",
                        House = "10",
                        Postcode = "TA6 2NN",
                        PhoneNumber = "01234567890"
                    }, 3).SetName("CheckoutSucceeds_WithValidDistance_3-");
                yield return new TestCaseData(
                    new Customer
                    {
                        ID = 1,
                        Name = "Jane Doe",
                        StreetAddress = "Some Street",
                        House = "10",
                        Postcode = "TA6 8NN",
                        PhoneNumber = "01234567890"
                    }, 3).SetName("CheckoutSucceeds_WithValidDistance_3+");
                yield return new TestCaseData(
                    new Customer
                    {
                        ID = 1,
                        Name = "Jane Doe",
                        StreetAddress = "Some Street",
                        House = "10",
                        Postcode = "TA6 1NN",
                        PhoneNumber = "01234567890"
                    }, 4).SetName("CheckoutSucceeds_WithValidDistance_4-");
                yield return new TestCaseData(
                    new Customer
                    {
                        ID = 1,
                        Name = "Jane Doe",
                        StreetAddress = "Some Street",
                        House = "10",
                        Postcode = "TA6 0NN",
                        PhoneNumber = "01234567890"
                    }, 4).SetName("CheckoutSucceeds_WithValidDistance_4+");
                yield return new TestCaseData(
                    new Customer
                    {
                        ID = 1,
                        Name = "Jane Doe",
                        StreetAddress = "Some Street",
                        House = "10",
                        Postcode = "TA6 9NN",
                        PhoneNumber = "01234567890"
                    }, 4).SetName("CheckoutSucceeds_WithValidDistance_ZeroCase");
            }            
        }

        // Ensure distance is calculated correctly from postcode
        [Test]
        [TestCaseSource(nameof(CheckoutWithValidDistanceTestCases))]
        public void CheckoutSucceeds_WithValidDistance(Customer customer, int expectedDistance)
        {
            _customerViewModel.CurrentCustomer = customer;

            _customerRepository.Setup(c => c.UpdateCustomer(customer)).Returns(true);

            _orderRepository.Setup(o => o.UpdateDeliveryOrder(testOrderID, 1, expectedDistance))
                            .Returns(true);

            _customerViewModel.CheckoutCommand.Execute(null);

            _orderRepository.Verify(o => o.UpdateDeliveryOrder(testOrderID, 1, expectedDistance), Times.Once,
                                    "UpdateDeliveryOrder should be called with the expected distance");

            Assert.That(_customerViewModel.ErrorMessage, Is.EqualTo(""),
                        "Error message should be empty on successful order createion");
        }

        [Test]
        public void ErrorDisplayed_WhenOrderUpdate_Fails()
        {
            _customerViewModel.CurrentCustomer = testCustomer;

            _customerRepository.Setup(c => c.UpdateCustomer(testCustomer))
                               .Returns(true);

            _orderRepository.Setup(o => o.UpdateDeliveryOrder(testOrderID, 1, 0))
                            .Returns(false);

            _customerViewModel.CheckoutCommand.Execute(null);

            Assert.That(_customerViewModel.ErrorMessage, Is.EqualTo("Failed to update\n Order"),
                        "Error should be displayed when order update fails");
        }
    }
}
