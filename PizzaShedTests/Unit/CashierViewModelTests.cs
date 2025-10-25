using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PizzaShed.ViewModels;
using PizzaShed.Model;
using PizzaShed.Services.Data;
using Moq;
using NUnit;
using Accessibility;
using System.Collections.ObjectModel;

namespace PizzaShedTests.Unit
{    
    [TestFixture]
    public class CashierViewModelTests
    {
        private Mock<ISession> _session;
        private Mock<IProductRepository<Product>> _productRepository;
        private Mock<IProductRepository<Topping>> _toppingRepository;
        private Mock<IOrderRepository> _orderRepository;

        private CashierViewModel _cashierViewModel;

        // We create some test data to use here
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
        private static Topping _pepperoni =
            new Topping
            {
                ID = 3,
                Name = "Pepperoni",
                Price = (decimal)1.50,
                ChoiceRequired = false
            };
        private static Topping _onion =
            new Topping
            {
                ID = 4,
                Name = "Onion",
                Price = (decimal)1.00,
                ChoiceRequired = false                
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
                    RequiredChoices = {_tomatoBase}
                };
       
        private static Product _dealPizza =
                new Product
                {
                    ID = 1,
                    Category = "Pizza",
                    Name = "Placeholder Pizza",
                    Price = 0,
                    SizeName = "Small",
                    Toppings = [],
                    RequiredChoices = {_tomatoBase}
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
                    RequiredChoices = { _bbqBase}
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


        private static Product _dealDrink = 
            new Product 
            { 
                ID = 0, 
                Category = "Drink", 
                Name = "Placeholder Drink", 
                SizeName = "1.25l", 
                Price = 0,
                RequiredChoices = [],
                Toppings = []
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
        
        private static Product _dealSide = 
            new Product 
            { 
                ID = 4, 
                Category = "Side", 
                Name = "Required Choice Side",
                SizeName = "Regular",
                Price = 0,
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

        // Test Deal
        private static Product _testPizzaDeal = 
            new Product 
            { 
                ID = 1, 
                Category = "Deal", 
                Name = "Test Deal", 
                Price = (decimal)10.99,
                RequiredChoices = {_dealPizza, _dealDrink, _dealSide},
                Toppings = []
            };

        [SetUp]
        public void SetUp()
        {
            _session = new Mock<ISession>();
            _productRepository = new Mock<IProductRepository<Product>>();
            _toppingRepository = new Mock<IProductRepository<Topping>>();
            _orderRepository = new Mock<IOrderRepository>();
                        
            _productRepository.Setup(p => p.GetProductsByCategory("Pizza", "Small")).Returns(new List<Product> { _margheritaPizza, _bbqChickenPizza });
            _productRepository.Setup(p => p.GetProductsByCategory("Pizza", null)).Returns(new List<Product> { _margheritaPizza, _bbqChickenPizza });
            _productRepository.Setup(p => p.GetProductsByCategory("Pizza")).Returns(new List<Product> { _margheritaPizza, _bbqChickenPizza });
            _productRepository.Setup(p => p.GetProductsByCategory("Drink", "1.25l")).Returns(new List<Product> { _drink });
            _productRepository.Setup(p => p.GetProductsByCategory("Drink", null)).Returns(new List<Product> { _drink });
            _productRepository.Setup(p => p.GetProductsByCategory("Drink")).Returns(new List<Product> { _drink });
            _productRepository.Setup(p => p.GetProductsByCategory("Burger")).Returns(new List<Product> { _burger });
            _productRepository.Setup(p => p.GetProductsByCategory("Wrap")).Returns(new List<Product>());
            _productRepository.Setup(p => p.GetProductsByCategory("Side")).Returns(new List<Product> { _side });
            _productRepository.Setup(p => p.GetProductsByCategory("Side", "Regular")).Returns(new List<Product> { _side });
            _productRepository.Setup(p => p.GetProductsByCategory("Side", null)).Returns(new List<Product> { _side });
            _productRepository.Setup(p => p.GetMealDeals()).Returns(new List<Product> { _testPizzaDeal });
            _productRepository.Setup(p => p.GetProductsByCategory("Dip")).Returns(new List<Product>());

            var _pizzaToppings = new List<Topping> { _tomatoBase, _bbqBase, _pepperoni, _onion };
            _toppingRepository.Setup(t => t.GetProductsByCategory("Pizza", "Small")).Returns(_pizzaToppings);
            _toppingRepository.Setup(t => t.GetProductsByCategory("Pizza", null)).Returns(_pizzaToppings);
            _toppingRepository.Setup(t => t.GetProductsByCategory("Pizza")).Returns(_pizzaToppings);

            _session.Setup(s => s.CurrentUser).Returns(new User(1,"Test","Cashier"));

            _orderRepository.Setup(o => o.CreateOrder(It.IsAny<Order>())).Returns(1);

            _cashierViewModel = new CashierViewModel(_productRepository.Object, 
                                                     _toppingRepository.Object, 
                                                     _orderRepository.Object, 
                                                     _session.Object, 
                                                     []);
        }


        // We create an IEnumerable object of the type TestCaseData to test multiple cases with a single test
        public static IEnumerable<TestCaseData> SingleItemOrderTestCases
        {
            get
            {
                yield return new TestCaseData((Product)_margheritaPizza.Clone(), "£10.49")
                    .SetName("OrderTotal_DisplaysCorrectPrice_SingleItem_Margherita");
                yield return new TestCaseData((Product)_bbqChickenPizza.Clone(), "£12.99")
                    .SetName("OrderTotal_DisplaysCorrectPrice_SingleItem_BBQChicken");
                yield return new TestCaseData(_burger, "£12.00")
                    .SetName("OrderTotal_DisplaysCorrectPrice_SingleItem_Burger");
            }
        }        

        // We ensure the order Total displays as expected
        [Test]
        [TestCaseSource(nameof(SingleItemOrderTestCases))]
        public void OrderTotal_DisplaysCorrectPrice_SingleItem(Product product, string expectedTotal)
        {
            _cashierViewModel.CurrentOrderItems.Add(product);

            string actualTotal = _cashierViewModel.OrderTotal;

            Assert.That(actualTotal, Is.EqualTo(expectedTotal), "Order Total should be the price of a single item");
        }

        public static IEnumerable<TestCaseData> MultipleItemOrderTestCases
        {
            get
            {
                yield return new TestCaseData(new List<Product> { _margheritaPizza, _burger }, "£22.49")
                    .SetName("OrderTotal_DisplaysCorrectPrice_MultipleItems_MargheritaBurger");
                yield return new TestCaseData(new List<Product> { _margheritaPizza, _bbqChickenPizza }, "£23.48")
                    .SetName("OrderTotal_DisplaysCorrectPrice_MultipleItems_MargheritaBBQChicken");
                yield return new TestCaseData(new List<Product> { _margheritaPizza, _burger, _drink, _side }, "£26.73")
                    .SetName("OrderTotal_DisplaysCorrectPrice_MultipleItems_MargheritaBurgerDrinkSide");
            }
        }

        // We ensure the order Total displays as expected with multiple items
        [Test]
        [TestCaseSource(nameof(MultipleItemOrderTestCases))]
        public void OrderTotal_DisplaysCorrectPrice_MultipleItems(List<Product> products, string expectedTotal)
        {
            _cashierViewModel.CurrentOrderItems = [.. products];

            string actualTotal = _cashierViewModel.OrderTotal;

            Assert.That(actualTotal, Is.EqualTo(expectedTotal), "Order total should be the sum of current order prices");
        }

        public static IEnumerable<TestCaseData> SingleItemWithToppingTestCases
        {
            get
            {
                yield return new TestCaseData((Product)_margheritaPizza.Clone(), new List<Topping> { _onion }, "£11.49")
                    .SetName("OrderTotal_DisplaysCorrectPrice_SingleItem_WithToppings_Margherita_Onion");
                yield return new TestCaseData((Product)_margheritaPizza.Clone(), new List<Topping> { _onion, _pepperoni }, "£12.99")
                    .SetName("OrderTotal_DisplaysCorrectPrice_SingleItem_WithToppings_Margherita_Pepperoni_Onion");
                yield return new TestCaseData((Product)_bbqChickenPizza.Clone(), new List<Topping> { _onion }, "£13.99")
                    .SetName("OrderTotal_DisplaysCorrectPrice_SingleItem_WithToppings_BBQChicken_Onion");
                yield return new TestCaseData((Product)_bbqChickenPizza.Clone(), new List<Topping> { _onion, _pepperoni }, "£15.49")
                    .SetName("OrderTotal_DisplaysCorrectPrice_SingleItem_WithToppings_BBQChicken_Pepperoni_Onion");
            }
        }

        // We ensure that the price of items with toppings is calculated correctly
        [Test]
        [TestCaseSource(nameof(SingleItemWithToppingTestCases))]
        public void OrderTotal_DisplaysCorrectPrice_SingleItem_WithToppings(Product product, List<Topping> toppings, string expectedTotal)
        {
            product.Toppings = [.. toppings];

            _cashierViewModel.CurrentOrderItems.Add(product);

            string actualTotal = _cashierViewModel.OrderTotal;

            Assert.That(actualTotal, Is.EqualTo(expectedTotal), "Order Total should be the sum of the item price and the topping price(s)");
        }

        public static IEnumerable<TestCaseData> MultipleItemsWithToppings
        {
            get
            {
                yield return new TestCaseData(new List<Product> { (Product)_bbqChickenPizza.Clone(), (Product)_margheritaPizza.Clone() }, new List<Topping> { _onion }, "£25.48")
                    .SetName("OrderTotal_DisplaysCorrectPrice_MultipleItems_WithToppings_BBQChicken_Margherita_Onion");
                yield return new TestCaseData(new List<Product> { _bbqChickenPizza, _margheritaPizza }, new List<Topping> { _onion, _pepperoni }, "£28.48")
                    .SetName("OrderTotal_DisplaysCorrectPrice_MultipleItems_WithToppings_BBQChicken_Margherita_Onion_Pepperoni");
            }
        }
        
        // We ensure the order total is correct for multiple items with toppings
        [Test]
        [TestCaseSource(nameof(MultipleItemsWithToppings))]
        public void OrderTotal_DisplaysCorrectPrice_MultipleItems_WithToppings(List<Product> products, List<Topping> toppings, string expectedTotal)
        {
            foreach (Product p in products)
            {
                p.Toppings = [.. toppings];
            }

            _cashierViewModel.CurrentOrderItems = [.. products];

            string actualTotal = _cashierViewModel.OrderTotal;

            Assert.That(actualTotal, Is.EqualTo(expectedTotal), "Order total should be the sum of the price of products and their toppings");
        }

        public static IEnumerable<TestCaseData> RemoveItemTestCases
        {            
            get
            {
                Product firstItem = (Product)_margheritaPizza.Clone();
                Product secondItem = (Product)_bbqChickenPizza.Clone();
                Product thirdItem = (Product)_burger.Clone();

                List<Product> initialProducts = new(){ firstItem, secondItem, thirdItem };

                yield return new TestCaseData(initialProducts, thirdItem, secondItem)
                    .SetName("RemoveItem_LastItem_SelectsLast");
                yield return new TestCaseData(initialProducts, secondItem, thirdItem)
                    .SetName("RemoveItem_MiddleItem_SelectsLast");

                List<Product> singleItem = new() { firstItem };
                yield return new TestCaseData(singleItem, firstItem, null)
                    .SetName("RemoveItem_SingleItem_SelectsNull");
            }
        }

        // We ensure our remove item functionality with the Void button works as expected
        [Test]
        [TestCaseSource(nameof(RemoveItemTestCases))]
        public void RemoveItem(List<Product> products, Product removeProduct, Product? selectedProduct)
        {
            _cashierViewModel = new CashierViewModel(
                _productRepository.Object,
                _toppingRepository.Object,
                _orderRepository.Object,
                _session.Object,
                [.. products]
                );            

            _cashierViewModel.SelectedOrderItem = removeProduct;
            
            int initialCount = _cashierViewModel.CurrentOrderItems.Count;

            _cashierViewModel.RemoveOrderItemCommand.Execute(null);

            Assert.That(_cashierViewModel.CurrentOrderItems.Count, Is.EqualTo(initialCount - 1), "Only one item should be removed");
            Assert.That(_cashierViewModel.SelectedOrderItem, Is.EqualTo(selectedProduct), "The selected product should be the last product in the list - or null");
        }

        public static IEnumerable<TestCaseData> HalfAndHalfTestCases
        {
            get
            {
                yield return new TestCaseData((Product)_margheritaPizza.Clone(), (Product)_bbqChickenPizza.Clone(), "£12.99")
                    .SetName("HalfAndHalf_SelectsHigherPrice_Margherita_BBQ");
                yield return new TestCaseData((Product)_bbqChickenPizza.Clone(), (Product)_margheritaPizza.Clone(), "£12.99")
                    .SetName("HalfAndHalf_SelectsHigherPrice_BBQ_Margherita");
            }
        }

        [Test]
        [TestCaseSource(nameof(HalfAndHalfTestCases))]
        public void HalfAndHalf_SelectsHigherPrice(Product firstHalf, Product secondHalf, string expectedTotal)
        {
            _cashierViewModel.HalfAndHalfCommand.Execute(null);

            _cashierViewModel.AddOrderItemCommand.Execute(firstHalf);

            _cashierViewModel.AddOrderItemCommand.Execute(secondHalf);

            int orderCount = _cashierViewModel.CurrentOrderItems.Count;

            string actualTotal = _cashierViewModel.OrderTotal;

            Product expensiveHalf = firstHalf.Price > secondHalf.Price ? firstHalf : secondHalf;
            Product cheapHalf = firstHalf.Price < secondHalf.Price ? firstHalf : secondHalf;

            string expectedName = $"{expensiveHalf.Name} / {cheapHalf.Name}";

            Assert.That(orderCount, Is.EqualTo(1), "Half and half should add a single product");
            Assert.That(actualTotal, Is.EqualTo(expectedTotal), "Total should be the price of the more expensive half");
            Assert.That(_cashierViewModel.CurrentOrderItems.First().Name, Is.EqualTo(expectedName), "Product name should reflect a half and half pizza");
        }

        public static IEnumerable<TestCaseData> MealDealPriceTestCases
        {
            get
            {
                Product _deal = (Product)_testPizzaDeal.Clone();

                yield return new TestCaseData(_deal, "£10.99")
                    .SetName("DealTotal_BasePrice");                
            }
        }

        [Test]
        [TestCaseSource(nameof(MealDealPriceTestCases))]
        public void DealTotal(Product deal, string expectedTotal)
        {
            _cashierViewModel.AddOrderItemCommand.Execute(deal);

            string actualTotal = _cashierViewModel.OrderTotal;

            int orderCount = _cashierViewModel.CurrentOrderItems.Count;

            int expectedOrderCount = deal.RequiredChoices.Count + 1;

            string expectedMenuCategory = deal.RequiredChoices.OfType<Product>().First(p => p.ID == 0).Category;
            string actualMenuCategory = _cashierViewModel.SelectedCategory;

            Assert.That(actualTotal, Is.EqualTo(expectedTotal), "Deal total should be the base price of the deal and any toppings");
            Assert.That(orderCount, Is.EqualTo(expectedOrderCount), "Order should be populated with deal items");
            Assert.That(actualMenuCategory, Is.EqualTo(expectedMenuCategory), "ViewModel should navigate to the category of the next product that requires a choice");
        }

        public static IEnumerable<TestCaseData> RemoveDealItemTestCases
        {
            get
            {
                Product _deal = (Product)_testPizzaDeal.Clone();
                yield return new TestCaseData(_deal, 0)
                    .SetName("RemoveItemCommand_RemovesAllDealItems_Index_0");
                yield return new TestCaseData(_deal, 1)
                    .SetName("RemoveItemCommand_RemovesAllDealItems_Index_1");
                yield return new TestCaseData(_deal, 2)
                    .SetName("RemoveItemCommand_RemovesAllDealItems_Index_2");
                yield return new TestCaseData(_deal, 3)
                    .SetName("RemoveItemCommand_RemovesAllDealItems_Index_3");
            }
        }

        [Test]
        [TestCaseSource(nameof(RemoveDealItemTestCases))]
        public void RemoveItemCommand_RemovesAllDealItems(Product _dealItem, int index)
        {
            _cashierViewModel.AddOrderItemCommand.Execute(_dealItem);

            _cashierViewModel.RemoveOrderItemCommand.Execute(_cashierViewModel.CurrentOrderItems[index]);

            int orderCount = _cashierViewModel.CurrentOrderItems.Count;

            Assert.That(orderCount, Is.EqualTo(0), "Order should be empty");

            _orderRepository.Verify(o => o.CreateOrder(It.IsAny<Order>()), Times.Never, "New order should not be created when order empty after deal removal");
        }

        public static IEnumerable<TestCaseData> DeliveryBelowMinumumTestCases
        {
            get
            {
                yield return new TestCaseData(new List<Product>(), "£0.00")
                    .SetName("Delivery_BelowMinimumAmount_Error_EmptyOrder");
                yield return new TestCaseData(new List<Product>() { (Product)_margheritaPizza.Clone() }, "£10.49")
                    .SetName("Delivery_BelowMinumumAmount_Error_Margherita");
            }
        }

        // Ensure that an error is displayed when delivery order total is below £12
        [Test]
        [TestCaseSource(nameof(DeliveryBelowMinumumTestCases))] 
        public void Delivery_BelowMinumumAmount_Error(List<Product> products, string expectedTotal)
        {
            _cashierViewModel.CurrentOrderItems = [.. products];

            _cashierViewModel.IsDeliveryCommand.Execute(null);

            _cashierViewModel.CheckoutCommand.Execute(null);

            bool expectedError = products.Count == 0 ? false : true;
            bool actualError = _cashierViewModel.IsError;
            string actualTotal = _cashierViewModel.OrderTotal;

            Assert.That(actualError, Is.EqualTo(expectedError), "Error message should be displayed if order contains items");
            Assert.That(actualTotal, Is.EqualTo(expectedTotal), "Total should be the sum of the order item prices");

            _orderRepository.Verify(o => o.CreateOrder(It.IsAny<Order>()), Times.Never(), "Order Creation should not be called when below minumum total for delivery");
        }

        [Test]
        public void CollectionOrderCreated_OnCheckout_Pass()
        {
            List<Product> products = new() { (Product)_margheritaPizza.Clone(), (Product)_bbqChickenPizza.Clone() };
            
            _cashierViewModel.CurrentOrderItems = [.. products];

            _cashierViewModel.CheckoutCommand.Execute(null);

            _orderRepository.Verify(o => o.CreateOrder(It.IsAny<Order>()), Times.Once(), "New order should be created on checkout");
        }

        [Test]
        public void CollectionOrderCreated_OnCheckout_Fail()
        {
            _cashierViewModel.CheckoutCommand.Execute(null);
            
            _orderRepository.Verify(o => o.CreateOrder(It.IsAny<Order>()), Times.Never, "New order should not be created with an empty order");
        }
    }
}
