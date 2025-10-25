using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PizzaShed.Services.Data;
using PizzaShed.Model;
using PizzaShed.ViewModels;
using System.Collections.ObjectModel;

namespace PizzaShedTests.Unit
{
    public class OrderViewModelTests
    {
        private Mock<ISession> _session;
        private Mock<IOrderRepository> _orderRepository;
        private Mock<ICustomerRepository> _customerRepository;

        private OrderViewModel _orderViewModel;

        // Test data
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

        // We create a static dictionary to access Order's depending on role
        private static Dictionary<string, ObservableCollection<Order>> _orderDict = new Dictionary<string, ObservableCollection<Order>>
        {
            {"Cashier",
                new ObservableCollection<Order>
                {
                    new Order
                    {
                        ID = 1,
                        OrderStatus = "Preparing",
                        CollectionTime = DateTime.Now + new TimeSpan(00,15,00),
                        OrderType = "Collection",
                        OrderProducts = new ObservableCollection<Product>
                        {
                            _margheritaPizza,
                            _burger,
                            _drink
                        }
                    },
                    new Order
                    {
                        ID = 2,
                        OrderStatus = "Order Ready",
                        CollectionTime = DateTime.Now,
                        OrderType = "Collection",
                        OrderProducts = new ObservableCollection<Product>
                        {
                            _margheritaPizza,
                            _burger,
                            _drink
                        },
                        Paid = true
                    }
                }
            },
            {"Pizzaiolo",
                new ObservableCollection<Order>
                {
                    new Order
                    {
                        ID = 1,
                        OrderStatus = "New",
                        CollectionTime = DateTime.Now + new TimeSpan(00,15,00),
                        OrderType = "Collection",
                        OrderProducts = new ObservableCollection<Product>
                        {
                            _margheritaPizza,
                            _bbqChickenPizza
                        },
                        GrillReady = false
                    },
                    new Order
                    {
                        ID = 2,
                        OrderStatus = "Preparing",
                        CollectionTime = DateTime.Now,
                        OrderType = "Collection",
                        OrderProducts = new ObservableCollection<Product>
                        {
                            _margheritaPizza,
                            _burger,
                            _drink
                        },
                        GrillReady = true
                    },
                    new Order
                    {
                        ID = 3,
                        OrderStatus = "Preparing",
                        CollectionTime = DateTime.Now,
                        OrderType = "Collection",
                        OrderProducts = new ObservableCollection<Product>
                        {
                            _margheritaPizza,
                            _burger,
                            _drink
                        },
                        GrillReady = false
                    }
                }
            },
            {"Grill Cook",
                new ObservableCollection<Order>
                {
                    new Order
                    {
                        ID = 1,
                        OrderStatus = "New",
                        CollectionTime = DateTime.Now + new TimeSpan(00,15,00),
                        OrderType = "Collection",
                        OrderProducts = new ObservableCollection<Product>
                        {
                            _burger
                        },
                        PizzaReady = false
                    },
                    new Order
                    {
                        ID = 2,
                        OrderStatus = "Preparing",
                        CollectionTime = DateTime.Now,
                        OrderType = "Collection",
                        OrderProducts = new ObservableCollection<Product>
                        {
                            _burger,
                            _side
                        },
                        PizzaReady = true
                    },
                    new Order
                    {
                        ID = 3,
                        OrderStatus = "Preparing",
                        CollectionTime = DateTime.Now,
                        OrderType = "Collection",
                        OrderProducts = new ObservableCollection<Product>
                        {
                            _burger,
                            _side
                        },
                        PizzaReady = false
                    }
                }
            },
            {"Driver",
                new ObservableCollection<Order>
                {
                    new Order
                    {
                        ID = 1,
                        OrderStatus = "Order Ready",
                        OrderType = "Delivery",
                        OrderProducts = new ObservableCollection<Product>
                        {
                            _margheritaPizza,
                            _bbqChickenPizza
                        },
                        CustomerID = 1,
                        Customer = new Customer
                        {
                            ID = 1,
                            Name = "John Doe",
                            StreetAddress = "Sesame Street",
                            House = "10",
                            Postcode = "TA6 5NN",
                            PhoneNumber = "01234567890"
                        }
                    },
                    new Order
                    {
                        ID = 2,
                        OrderStatus = "Out For Delivery",
                        OrderType = "Delivery",
                        OrderProducts = new ObservableCollection<Product>
                        {
                            _margheritaPizza,
                            _burger,
                            _drink
                        },
                        CustomerID = 1,
                        Customer = new Customer
                        {
                            ID = 1,
                            Name = "John Doe",
                            StreetAddress = "Sesame Street",
                            House = "10",
                            Postcode = "TA6 5NN",
                            PhoneNumber = "01234567890"
                        },
                        Paid = true
                    }
                }
            }
        };

        [SetUp]
        public void SetUp()
        {
            _session = new Mock<ISession>();
            _orderRepository = new Mock<IOrderRepository>();
            _customerRepository = new Mock<ICustomerRepository>();

            _orderViewModel = new OrderViewModel(_session.Object, _orderRepository.Object, _customerRepository.Object);
        }

        private void SetUpMocks(string role, ObservableCollection<Order> initialOrders)
        {            
            _session.SetupGet(s => s.UserRole).Returns(() => role);            
        
            _orderRepository.Setup(o => o.GetCollectionOrders()).Returns(initialOrders);
            _orderRepository.Setup(o => o.GetKitchenOrders(true)).Returns(initialOrders);
            _orderRepository.Setup(o => o.GetKitchenOrders(false)).Returns(initialOrders);
            _orderRepository.Setup(o => o.GetDeliveryOrders()).Returns(initialOrders);

            // We need to create the OrderViewModel again to reflect the changes
            _orderViewModel = new OrderViewModel(_session.Object, _orderRepository.Object, _customerRepository.Object);
        }

        [TestCase("Cashier", true, false, false)]
        [TestCase("Pizzaiolo", false, true, false)]
        [TestCase("Grill Cook", false, true, false)]
        [TestCase("Driver", false, false, true)]
        public void Properties_Initialized_Correctly_OnRole(string role, bool isCashier, bool isCook, bool isDriver)
        {
            SetUpMocks(role, _orderDict[role]);

            Assert.That(_orderViewModel.IsCashier, Is.EqualTo(isCashier));
            Assert.That(_orderViewModel.IsCook, Is.EqualTo(isCook));
            Assert.That(_orderViewModel.IsDriver, Is.EqualTo(isDriver));
        }

        [TestCase("Cashier", 1, 1)]
        [TestCase("Pizzaiolo", 1, 2)]
        [TestCase("Grill Cook", 1, 2)]
        [TestCase("Driver", 1, 1)]
        public void UpdateView_FiltersOrders_BasedOnStatus(string role, int newCount, int readyCount)
        {
            SetUpMocks(role, _orderDict[role]);            

            Assert.That(_orderViewModel.NewOrders, Has.Count.EqualTo(newCount));
            Assert.That(_orderViewModel.ReadyOrders, Has.Count.EqualTo(readyCount));
        }

        [Test]
        public void UpdateView_Driver_FetchesCustomers()
        {
            SetUpMocks("Driver", _orderDict["Driver"]);

            _customerRepository.Verify(c => c.GetCustomerByID(It.IsAny<int>()), Times.Exactly(_orderDict["Driver"].Count));
        }

        // We already test navigation functionality on unpaid Delivery/Collection orders in MainViewModelTests
        // so we only test paid orders here

        [Test]
        public void CompleteOrder_Driver_OrderReady_CallsDeliverOrder()
        {
            SetUpMocks("Driver", _orderDict["Driver"]);

            _orderViewModel.CompleteOrderCommand.Execute(1);

            _orderRepository.Verify(o => o.DeliverOrder(1), Times.Once);
            // We can check if the view was updated with the calls to GetDeliveryOrders
            _orderRepository.Verify(o => o.GetDeliveryOrders(), Times.Exactly(2));
        }

        [Test]
        public void CompleteOrder_Driver_OutForDelivery_CallsCompleteOrder()
        {
            SetUpMocks("Driver", _orderDict["Driver"]);

            _orderViewModel.CompleteOrderCommand.Execute(2);

            _orderRepository.Verify(o => o.CompleteOrder(2), Times.Once);
            _orderRepository.Verify(o => o.GetDeliveryOrders(), Times.Exactly(2));
        }

        [Test]
        public void CompleteOrder_Cashier_OrderReady_CallsCompleteOrder()
        {
            SetUpMocks("Cashier", _orderDict["Cashier"]);

            _orderViewModel.CompleteOrderCommand.Execute(2);

            _orderRepository.Verify(o => o.CompleteOrder(2), Times.Once);
            _orderRepository.Verify(o => o.GetCollectionOrders(), Times.Exactly(2));
        }

        [TestCase("Pizzaiolo", true)]
        [TestCase("Grill Cook", false)]
        public void CompleteOrder_Kitchen_New_CallsPrepareOrder(string role, bool pizzas)
        {
            SetUpMocks(role, _orderDict[role]);

            _orderViewModel.CompleteOrderCommand.Execute(1);

            _orderRepository.Verify(o => o.PrepareOrder(1), Times.Once);
            _orderRepository.Verify(o => o.CompleteOrder(1), Times.Never);
            _orderRepository.Verify(o => o.GetKitchenOrders(pizzas), Times.Exactly(2));
        }

        [Test]
        public void CompleteOrder_Pizzaiolo_Preparing_GrillsReady_CallsOrderReady()
        {
            SetUpMocks("Pizzaiolo", _orderDict["Pizzaiolo"]);

            _orderViewModel.CompleteOrderCommand.Execute(2);

            _orderRepository.Verify(o => o.OrderReady(2), Times.Once);
            _orderRepository.Verify(o => o.CompleteOrder(2), Times.Never);
            _orderRepository.Verify(o => o.GetKitchenOrders(true), Times.Exactly(3));
        }

        [Test]
        public void CompleteOrder_GrillCook_Preparing_PizzaReady_CallsOrderReady()
        {
            SetUpMocks("Grill Cook", _orderDict["Grill Cook"]);

            _orderViewModel.CompleteOrderCommand.Execute(2);

            _orderRepository.Verify(o => o.OrderReady(2), Times.Once);
            _orderRepository.Verify(o => o.CompleteOrder(2), Times.Never);
            _orderRepository.Verify(o => o.GetKitchenOrders(false), Times.Exactly(3));
        }

        [Test]
        public void CompleteOrder_Pizzaiolo_Preparing_GrillNotReady_CallsCompleteOrderStation()
        {
            SetUpMocks("Pizzaiolo", _orderDict["Pizzaiolo"]);

            _orderViewModel.CompleteOrderCommand.Execute(3);

            _orderRepository.Verify(o => o.CompleteOrderStation(3, true), Times.Once);
            _orderRepository.Verify(o => o.CompleteOrder(3), Times.Never);
            _orderRepository.Verify(o => o.GetKitchenOrders(true), Times.Exactly(3));
        }

        [Test]
        public void CompleteOrder_GrillCook_Preparing_PizzaNotReady_CallsCompleteOrderStation()
        {
            SetUpMocks("Grill Cook", _orderDict["Grill Cook"]);

            _orderViewModel.CompleteOrderCommand.Execute(3);

            _orderRepository.Verify(o => o.CompleteOrderStation(3, false), Times.Once);
            _orderRepository.Verify(o => o.CompleteOrder(3), Times.Never);
            _orderRepository.Verify(o => o.GetKitchenOrders(false), Times.Exactly(3));
        }

        [TestCase("Cashier")]
        [TestCase("Pizzaiolo")]
        [TestCase("Grill Cook")]
        [TestCase("Driver")]
        public void Logout_Calls_SessionLogout(string role)
        {
            SetUpMocks(role, _orderDict[role]);

            _orderViewModel.LogoutCommand.Execute(null);

            _session.Verify(s => s.Logout(), Times.Once);   
        }
    }
}
