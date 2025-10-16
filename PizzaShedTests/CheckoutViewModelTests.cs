using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using PizzaShed.Model;
using PizzaShed.ViewModels;
using PizzaShed.Services.Data;
using NuGet.Frameworks;
using NUnit.Framework.Constraints;

namespace PizzaShedTests
{
    [TestFixture]
    public class CheckoutViewModelTests
    {
        private Mock<IOrderRepository> _orderRepository;
        private Mock<ISession> _session;
        private const int _testOrderID = 1;


        // Test Data
        private static Topping _tomatoBase =
            new Topping
            {
                ID = 1,
                Name = "Tomato",
                Price = 0,
                ChoiceRequired = true
            };

        private static Topping _bbqBase =
            new Topping
            {
                ID = 2,
                Name = "BBQ",
                Price = 0,
                ChoiceRequired = true
            };

        private static Product _margheritaPizza =
                new Product
                {
                    ID = 1,
                    Category = "Pizza",
                    Name = "Margherita",
                    Price = (decimal)10.49,
                    SizeName = "Small",
                    Toppings = [],
                    RequiredChoices = { _tomatoBase }
                };

        private static Product _bbqChickenPizza =
                new Product
                {
                    ID = 2,
                    Category = "Pizza",
                    Name = "BBQ Chicken",
                    Price = (decimal)12.99,
                    SizeName = "Small",
                    Toppings = [],
                    RequiredChoices = { _bbqBase }
                };
        private static Product _burger =
                new Product
                {
                    ID = 3,
                    Category = "Burger",
                    Name = "Burger",
                    Price = (decimal)12.00,
                    SizeName = "Regular",
                    Toppings = [],
                    RequiredChoices = []
                };

        private static Product _drink =
            new Product
            {
                ID = 0,
                Category = "Drink",
                Name = "Drink",
                SizeName = "1.25l",
                Price = (decimal)1.25,
                RequiredChoices = [],
                Toppings = []
            };

        private static Product _side =
            new Product
            {
                ID = 4,
                Category = "Side",
                Name = "Side",
                SizeName = "Regular",
                Price = (decimal)2.99,
                RequiredChoices = [],
                Toppings = []
            };

        // We only set up the repositories - we will manually create the ViewModel every test
        [SetUp]
        public void SetUp()
        {
            _orderRepository = new Mock<IOrderRepository>();
            _session = new Mock<ISession>();
        }

        // We create methods for our different order test types
        private Order SetUpNewDeliveryOrderNoPromo()
        {
            Order order = new Order
            {
                ID = 1,
                OrderStatus = "New",
                OrderProducts = new ObservableCollection<Product> { _burger },
                Paid = false,
                CustomerID = 1,
                OrderType = "Delivery",
                UserID = 1,
                DeliveryFee = 2.00m
            };

            _orderRepository.Setup(o => o.GetOrderByOrderNumber(1)).Returns(order);
            _orderRepository.Setup(o => o.FetchEligiblePromotions(It.IsAny<decimal>())).Returns([]);
            _orderRepository.Setup(o => o.GetDeliveryTime()).Returns((true, "07:00 PM"));
            _session.SetupGet(s => s.UserRole).Returns(() => "Cashier");

            return order;
        }

        private Order SetUpNewDeliveryOrderWithPromo()
        {
            Order order = new Order
            {
                ID = 1,
                OrderStatus = "New",
                OrderProducts = new ObservableCollection<Product> { _burger, _drink, _side },
                Paid = false,
                CustomerID = 1,
                OrderType = "Delivery",
                UserID = 1,
                DeliveryFee = 2.00m
            };

            _orderRepository.Setup(o => o.GetOrderByOrderNumber(1)).Returns(order);
            _orderRepository.Setup(o => o.FetchEligiblePromotions(It.IsAny<decimal>()))
                                         .Returns(new ObservableCollection<Promotion>
                                         {
                                             new Promotion{
                                                 ID = 1,
                                                 Description = "Test Promotion",
                                                 DiscountValue = 0.1m,
                                                 PromoCode = "TESTING"
                                             }
                                         });
            _orderRepository.Setup(o => o.GetDeliveryTime()).Returns((true, "07:00 PM"));
            _session.SetupGet(s => s.UserRole).Returns(() => "Cashier");

            return order;
        }

        private Order SetUpNewCollectionOrderNoPromo()
        {
            Order order = new Order
            {
                ID = 1,
                OrderStatus = "New",
                OrderProducts = new ObservableCollection<Product> { _margheritaPizza },
                Paid = false,                
                OrderType = "Collection",
                UserID = 1
            };

            _orderRepository.Setup(o => o.GetOrderByOrderNumber(1)).Returns(order);
            _orderRepository.Setup(o => o.FetchEligiblePromotions(It.IsAny<decimal>())).Returns([]);
            _orderRepository.Setup(o => o.GetCollectionTimes()).Returns((true, ["07:00 PM", "07:15 PM", "07:30 PM", "07:45 PM"]));
            _session.SetupGet(s => s.UserRole).Returns(() => "Cashier");

            return order;
        }

        private Order SetUpNewCollectionOrderWithPromo()
        {
            Order order = new Order
            {
                ID = 1,
                OrderStatus = "New",
                OrderProducts = new ObservableCollection<Product> { _burger, _side, _drink},
                Paid = false,                
                OrderType = "Collection",
                UserID = 1
            };

            _orderRepository.Setup(o => o.GetOrderByOrderNumber(1)).Returns(order);
            _orderRepository.Setup(o => o.FetchEligiblePromotions(It.IsAny<decimal>()))
                                         .Returns(new ObservableCollection<Promotion>
                                         {
                                             new Promotion{
                                                 ID = 1,
                                                 Description = "Test Promotion",
                                                 DiscountValue = 0.1m,
                                                 PromoCode = "TESTING"
                                             }
                                         });
            _orderRepository.Setup(o => o.GetCollectionTimes()).Returns((true, ["07:00 PM", "07:15 PM", "07:30 PM", "07:45 PM"]));
            _session.SetupGet(s => s.UserRole).Returns(() => "Cashier");

            return order;
        }

        private Order SetUpUnpaidDeliveryOrder()
        {
            Order order = new Order
            {
                ID = 1,
                OrderStatus = "Out For Delivery",
                OrderProducts = new ObservableCollection<Product> { _burger, _side, _drink },
                Paid = false,
                OrderType = "Delivery",
                UserID = 1,
                DeliveryFee = 2.00m
            };

            _orderRepository.Setup(o => o.GetOrderByOrderNumber(1)).Returns(order);
            _orderRepository.Setup(o => o.FetchEligiblePromotions(It.IsAny<decimal>()))
                                         .Returns(new ObservableCollection<Promotion>
                                         {
                                             new Promotion{
                                                 ID = 1,
                                                 Description = "Test Promotion",
                                                 DiscountValue = 0.1m,
                                                 PromoCode = "TESTING"
                                             }
                                         });

            _orderRepository.Setup(o => o.GetDeliveryTime()).Returns((true, "07:00 PM"));
            _session.SetupGet(s => s.UserRole).Returns(() => "Driver");

            return order;
        }

        private Order SetUpUnpaidCollectionOrder()
        {
            Order order = new Order
            {
                ID = 1,
                OrderStatus = "Order Ready",
                OrderProducts = new ObservableCollection<Product> { _burger, _side, _drink },
                Paid = false,                
                OrderType = "Collection",
                UserID = 1
            };

            _orderRepository.Setup(o => o.GetOrderByOrderNumber(1)).Returns(order);
            _orderRepository.Setup(o => o.FetchEligiblePromotions(It.IsAny<decimal>()))
                                         .Returns(new ObservableCollection<Promotion>
                                         {
                                             new Promotion{
                                                 ID = 1,
                                                 Description = "Test Promotion",
                                                 DiscountValue = 0.1m,
                                                 PromoCode = "TESTING"
                                             }
                                         });
            _orderRepository.Setup(o => o.GetCollectionTimes()).Returns((true, ["07:00 PM", "07:15 PM", "07:30 PM", "07:45 PM"]));
            _session.SetupGet(s => s.UserRole).Returns(() => "Cashier");

            return order;
        }

        [Test]
        public void NewDeliveryOrder_Constructor_Initializes_ExpectedState_NoPromo()
        {
            SetUpNewDeliveryOrderNoPromo();

            CheckoutViewModel _checkoutViewModel = new CheckoutViewModel(_orderRepository.Object, _session.Object, _testOrderID);

            Assert.That(_checkoutViewModel.IsDelivery, Is.True);
            Assert.That(_checkoutViewModel.AcceptOrder, Is.True);
            Assert.That(_checkoutViewModel.Promotions, Has.Count.EqualTo(0));
            Assert.That(_checkoutViewModel.EligibleForPromotion, Is.False);
            Assert.That(_checkoutViewModel.OrderSource, Is.EqualTo("Phone"));
            Assert.That(_checkoutViewModel.NotesVisible, Is.True);
        }

        [Test]
        public void NewDeliveryOrder_Constructor_Initializes_ExpectedState_WithPromo()
        {
            SetUpNewDeliveryOrderWithPromo();

            CheckoutViewModel _checkoutViewModel = new CheckoutViewModel(_orderRepository.Object, _session.Object, _testOrderID);

            Assert.That(_checkoutViewModel.IsDelivery, Is.True);
            Assert.That(_checkoutViewModel.IsCollection, Is.False);
            Assert.That(_checkoutViewModel.AcceptOrder, Is.True);
            Assert.That(_checkoutViewModel.Promotions, Has.Count.EqualTo(1));
            Assert.That(_checkoutViewModel.EligibleForPromotion, Is.True);
            Assert.That(_checkoutViewModel.OrderSource, Is.EqualTo("Phone"));
            Assert.That(_checkoutViewModel.NotesVisible, Is.True);
        }

        [Test]
        public void NewCollectionOrder_Constructor_Initializes_ExpectedState_NoPromo()
        {
            SetUpNewCollectionOrderNoPromo();            

            CheckoutViewModel _checkoutViewModel = new CheckoutViewModel(_orderRepository.Object, _session.Object, _testOrderID);

            Assert.That(_checkoutViewModel.IsDelivery, Is.False);
            Assert.That(_checkoutViewModel.IsCollection, Is.True);
            Assert.That(_checkoutViewModel.IsPhone, Is.False);
            Assert.That(_checkoutViewModel.AcceptOrder, Is.True);
            Assert.That(_checkoutViewModel.CollectionTimes, Has.Count.GreaterThan(0));
            Assert.That(_checkoutViewModel.SelectedCollectionTime, Is.EquivalentTo(_checkoutViewModel.CollectionTimes.First()));
            Assert.That(_checkoutViewModel.Promotions, Has.Count.EqualTo(0));
            Assert.That(_checkoutViewModel.EligibleForPromotion, Is.False);
            Assert.That(_checkoutViewModel.NotesVisible, Is.True);
        }

        [Test]
        public void NewCollectionOrder_Constructor_Initializes_ExpectedState_WithPromo()
        {
            SetUpNewCollectionOrderWithPromo();

            CheckoutViewModel _checkoutViewModel = new CheckoutViewModel(_orderRepository.Object, _session.Object, _testOrderID);

            Assert.That(_checkoutViewModel.IsDelivery, Is.False);
            Assert.That(_checkoutViewModel.IsCollection, Is.True);
            Assert.That(_checkoutViewModel.IsPhone, Is.False);
            Assert.That(_checkoutViewModel.AcceptOrder, Is.True);
            Assert.That(_checkoutViewModel.CollectionTimes, Has.Count.GreaterThan(0));
            Assert.That(_checkoutViewModel.SelectedCollectionTime, Is.EquivalentTo(_checkoutViewModel.CollectionTimes.First()));
            Assert.That(_checkoutViewModel.Promotions, Has.Count.EqualTo(1));
            Assert.That(_checkoutViewModel.EligibleForPromotion, Is.True);
            Assert.That(_checkoutViewModel.NotesVisible, Is.True);
        }

        [Test]
        public void UnpaidDeliveryOrder_Constructor_Initializes_ExpectedState()
        {
            SetUpUnpaidDeliveryOrder();

            CheckoutViewModel _checkoutViewModel = new CheckoutViewModel(_orderRepository.Object, _session.Object, _testOrderID);

            Assert.That(_checkoutViewModel.IsDelivery, Is.True);
            Assert.That(_checkoutViewModel.IsCollection, Is.False);
            Assert.That(_checkoutViewModel.IsPhone, Is.False);
            Assert.That(_checkoutViewModel.AcceptOrder, Is.False);                        
            Assert.That(_checkoutViewModel.Promotions, Has.Count.EqualTo(0));
            Assert.That(_checkoutViewModel.EligibleForPromotion, Is.False);
            Assert.That(_checkoutViewModel.IsDriverCash, Is.True);
            Assert.That(_checkoutViewModel.NotesVisible, Is.False);
        }

        [Test]
        public void UnpaidCollectionOrder_Constructor_Initializes_ExpectedState()
        {
            SetUpUnpaidCollectionOrder();

            CheckoutViewModel _checkoutViewModel = new CheckoutViewModel(_orderRepository.Object, _session.Object, _testOrderID);

            Assert.That(_checkoutViewModel.IsDelivery, Is.False);
            Assert.That(_checkoutViewModel.IsCollection, Is.True);
            Assert.That(_checkoutViewModel.IsPhone, Is.False);
            Assert.That(_checkoutViewModel.AcceptOrder, Is.True);
            Assert.That(_checkoutViewModel.Promotions, Has.Count.EqualTo(0));
            Assert.That(_checkoutViewModel.EligibleForPromotion, Is.False);
            Assert.That(_checkoutViewModel.NotesVisible, Is.False);
        }

        [Test]
        public void DiscountValue_FormatCorrect_OnPromotion_Collection()
        {
            Order order = SetUpNewCollectionOrderWithPromo();

            CheckoutViewModel _checkoutViewModel = new CheckoutViewModel(_orderRepository.Object, _session.Object, _testOrderID);

            _checkoutViewModel.SelectedPromotion = _checkoutViewModel.Promotions.First();

            decimal orderTotal = order.PriceAfterPayments;

            decimal discountValue = Math.Round(orderTotal * _checkoutViewModel.SelectedPromotion.DiscountValue, 2);

            Assert.That(_checkoutViewModel.DiscountValue, Is.EqualTo($"-£{discountValue:N2}"));
        }

        [Test]
        public void DiscountValue_FormatCorrect_OnPromotion_Delivery()
        {
            Order order = SetUpNewDeliveryOrderWithPromo();

            CheckoutViewModel _checkoutViewModel = new CheckoutViewModel(_orderRepository.Object, _session.Object, _testOrderID);

            _checkoutViewModel.SelectedPromotion = _checkoutViewModel.Promotions.First();
            
            decimal deliveryFee = order.DeliveryFee != null ? (decimal)order.DeliveryFee : 0;

            decimal orderTotal = order.TotalPrice - deliveryFee;

            decimal discountValue = Math.Round(orderTotal * _checkoutViewModel.SelectedPromotion.DiscountValue, 2);

            Assert.That(_checkoutViewModel.DiscountValue, Is.EqualTo($"-£{discountValue:N2}"));
        }

        [Test]
        public void SelectPhoneCommand_Toggles_IsPhone_And_OrderSource()
        {
            SetUpNewCollectionOrderNoPromo();

            CheckoutViewModel _checkoutViewModel = new CheckoutViewModel(_orderRepository.Object, _session.Object, _testOrderID);

            _checkoutViewModel.SelectPhoneCommand.Execute(null);

            Assert.That(_checkoutViewModel.IsPhone, Is.True);
            Assert.That(_checkoutViewModel.OrderSource, Is.EqualTo("Phone"));
        }

        [Test]
        public void DeliveryOrder_OnCash_SimulatesFullPayment()
        {
            Order order = SetUpNewDeliveryOrderNoPromo();

            decimal totalPrice = order.TotalPrice;

            CheckoutViewModel _checkoutViewModel = new CheckoutViewModel(_orderRepository.Object, _session.Object, _testOrderID);

            _checkoutViewModel.CashCommand.Execute(null);

            Assert.That(order.Payments["Cash"], Has.Count.EqualTo(1));
            Assert.That(order.Payments["Cash"].First(), Is.EqualTo(totalPrice));
            Assert.That(_checkoutViewModel.IsPaid, Is.True);            
        }

        [Test]
        public void CollectionOrder_IsPhone_OnCash_SimulatesFullPayment()
        {
            Order order = SetUpNewCollectionOrderNoPromo();

            decimal totalPrice = order.TotalPrice;

            CheckoutViewModel _checkoutViewModel = new CheckoutViewModel(_orderRepository.Object, _session.Object, _testOrderID);

            _checkoutViewModel.SelectPhoneCommand.Execute(null);

            _checkoutViewModel.CashCommand.Execute(null);

            Assert.That(order.Payments["Cash"], Has.Count.EqualTo(1));
            Assert.That(order.Payments["Cash"].First(), Is.EqualTo(totalPrice));
            Assert.That(_checkoutViewModel.IsPaid, Is.True);
        }

        public void MakePayment_SuccessfulPartialPayment_UpdatesOrder()
        {
            Order order = SetUpNewCollectionOrderNoPromo();

            decimal totalPrice = order.TotalPrice;
            
            // we simulate a split paymount without paying the full amount
            decimal firstPayment = order.TotalPrice - 1;

            CheckoutViewModel _checkoutViewModel = new CheckoutViewModel(_orderRepository.Object, _session.Object, _testOrderID);

            _checkoutViewModel.MakePayment(firstPayment);

            // Payments are cash by default
            _orderRepository.Verify(o => o.CreatePayment(_testOrderID, firstPayment, "Cash"), Times.Once);
            Assert.That(order.Payments["Cash"], Has.Count.EqualTo(1));
            Assert.That(order.Payments["Cash"].First(), Is.EqualTo(firstPayment));
            Assert.That(_checkoutViewModel.IsPaid, Is.False);
            Assert.That(order.TotalPrice , Is.EqualTo(1));
        }

        public void MakePayment_MultiplePartialPayments_And_OnCompleteOrder_UpdatesOrder()
        {
            Order order = SetUpNewCollectionOrderNoPromo();

            decimal totalPrice = order.TotalPrice;

            decimal firstPayment = totalPrice - 1;

            CheckoutViewModel _checkoutViewModel = new CheckoutViewModel(_orderRepository.Object, _session.Object, _testOrderID);

            _checkoutViewModel.MakePayment(firstPayment);
            _checkoutViewModel.MakePayment(1);

            _orderRepository.Verify(o => o.CreatePayment(_testOrderID, firstPayment, "Cash"), Times.Once);
            _orderRepository.Verify(o => o.CreatePayment(_testOrderID, 1, "Cash"), Times.Once);

            Assert.That(order.Payments["Cash"], Has.Count.EqualTo(2));
            Assert.That(order.Payments["Cash"].Sum(), Is.EqualTo(totalPrice));
            Assert.That(_checkoutViewModel.IsPaid, Is.True);

            _checkoutViewModel.CompleteOrderCommand.Execute(null);

            _orderRepository.Verify(o => o.UpdatePaidOrder(order), Times.Once);                
        }

        public void CompleteOrderCommand_OrderReady_CompletesOrder()
        {
            Order order = SetUpUnpaidCollectionOrder();

            decimal totalPrice = order.TotalPrice;

            CheckoutViewModel _checkoutViewModel = new CheckoutViewModel(_orderRepository.Object, _session.Object, _testOrderID);

            _checkoutViewModel.MakePayment(totalPrice);

            _checkoutViewModel.CompleteOrderCommand.Execute(null);

            _orderRepository.Verify(o => o.CompleteOrder(_testOrderID), Times.Once);            
        }

        public void BackCommand_DeletesOrder()
        {
            Order order = SetUpNewCollectionOrderNoPromo();

            CheckoutViewModel _checkoutViewModel = new CheckoutViewModel(_orderRepository.Object, _session.Object, _testOrderID);

            _checkoutViewModel.BackCommand.Execute(null);

            _orderRepository.Verify(o => o.DeleteOrder(_testOrderID), Times.Once);
        }

        public void Logout_DeletesOrder_AndCallsSessionLogout()
        {
            SetUpUnpaidCollectionOrder();

            CheckoutViewModel _checkoutViewModel = new CheckoutViewModel(_orderRepository.Object, _session.Object, _testOrderID);

            _checkoutViewModel.LogoutCommand.Execute(null);

            _orderRepository.Verify(o => o.DeleteOrder(_testOrderID), Times.Once);
            _session.Verify(s => s.Logout(), Times.Once);
        }
    }
}
