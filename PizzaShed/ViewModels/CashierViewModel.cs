using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using PizzaShed.Commands;
using PizzaShed.Model;
using PizzaShed.Services.Data;
using PizzaShed.Services.Logging;

namespace PizzaShed.ViewModels
{
    public class CashierViewModel : ViewModelBase
    {
        private readonly IProductRepository<Product> _productRepo;
        private readonly IProductRepository<Topping> _toppingRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly ISession _session;
        private Order? _currentOrder = null;

        //------        ORDER       ------//
        public ICommand AddOrderItemCommand { get; }
                    
        public ICommand RemoveOrderItemCommand { get; }     
        
        public int OrderID { 
            get
            {
                if (_currentOrder != null)
                {
                    return _currentOrder.ID;
                }
                return 0;
            } 
        }

        // Property to hold our order type
        private bool _isDelivery;
        public bool IsDelivery
        {
            get => _isDelivery;
            set => SetProperty(ref _isDelivery, value);
        }

        public ICommand IsDeliveryCommand { get; }

        private ObservableCollection<Product> _currentOrderItems;

        public ObservableCollection<Product> CurrentOrderItems
        {
            get => _currentOrderItems;
            set => SetProperty(ref _currentOrderItems, value);
        }

        // This field returns the total cost for use with order creation
        private decimal _totalCost
        {
            get
            {
                decimal totalCost = 0;

                if (CurrentOrderItems.Count > 0)
                {                    
                    CurrentOrderItems
                        .ToList() // We convert to list as ObservableCollection's have no ForEach function
                        .ForEach(item =>
                        {
                            // If we have a deal item we need to get the cost of every topping in the deal
                            if (item.Category.Equals("Deal", StringComparison.OrdinalIgnoreCase))
                            {
                                item.RequiredChoices.ToList()
                                    .ForEach(dealItem =>
                                    {
                                        if (dealItem is Product p)
                                        {
                                            totalCost += p.Toppings.Sum(t => t.Price);
                                        }
                                    });
                            }
                            // Check all categories so if paid toppings outside of the Pizza category are introducted later
                            // our current implementation will handle these changes
                            else if (item.Toppings.Count > 0)
                            {
                                totalCost += item.Toppings.Sum(t => t.Price);
                            }
                            // Now we can get the base price of the order item
                            totalCost += item.Price;
                        });                    
                }

                if (IsError && totalCost > (decimal)12.0)
                    IsError = false;

                return totalCost;
            }
        }
        
        // This property displays the total cost of the order to the user
        public string OrderTotal => $"£{_totalCost:N2}";


        // Property to hold the order item the user has selected
        private Product? _selectedOrderItem;
        public Product? SelectedOrderItem
        {
            get => _selectedOrderItem;
            set
            {
                if (SetProperty(ref _selectedOrderItem, value))
                {                    
                    if (SelectedOrderItem != null)
                    {
                        // Toppings are toggled for menu items - to improve UX we display the correct menu in the view (i.e. Medium Pizza) 
                        // when the user selects an order item from the ListView                        
                        SelectCategory(SelectedOrderItem.Category);
                        if (SelectedOrderItem.Category.Equals("Pizza", StringComparison.OrdinalIgnoreCase))
                        {
                            SelectSize(SelectedOrderItem.SizeName);
                        }
                    }
                }
            }
        }

        private Product? _activeDealParent = null;
        private int _currentDealGroupID = 0;

        private bool _isHalfAndHalf;
        public bool IsHalfAndHalf
        {
            get => _isHalfAndHalf;
            set => SetProperty(ref _isHalfAndHalf, value);
        }

        // Variable to temporarily store the first half when creating a HalfAndHalf pizza
        private Product? _tempFirstHalf = null;

        public ICommand HalfAndHalfCommand { get; }

        public ICommand CheckoutCommand { get;  }

        //------        MENU        ------//
        public ICommand SelectCategoryCommand { get; }
        public ICommand SelectSizeCommand { get; }

        // Property to hold the current menu category to display
        private string _selectedCategory;
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    // This updates the views style depending on whether the menu category need to display toppings
                    OnPropertyChanged(nameof(DisplayToppings));                    
                }
            }
        }

        // Property to hold the products shown to the user
        private ObservableCollection<Product> _currentProductMenu;
        public ObservableCollection<Product> CurrentProductMenu
        {
            get => _currentProductMenu;
            set => SetProperty(ref _currentProductMenu, value);
        }

        // Property that toggles the size selection and 50/50 buttons for Pizzas
        private bool _isPizza;
        public bool IsPizza
        {
            get => _isPizza;
            set => SetProperty(ref _isPizza, value);
        }

        // Property to hold the size menu selected by the user when pizza menu is displayed
        private string? _currentSizeSelection;
        public string? CurrentSizeSelection
        {
            get => _currentSizeSelection;
            set => SetProperty(ref _currentSizeSelection, value);
        }

        // Property decides whether toppings are displayed or not - manages view styling
        public bool DisplayToppings
        {
            get
            {
                return SelectedCategory.ToLower() switch
                {
                    "pizza" or "kebab" => true,
                    _ => false,
                };
            }
        }

        // Property to hold our available toppings - filters the current/default required choice (Pizza Base / Kebab Bread)
        private ObservableCollection<Topping> _currentToppingMenu;
        public ObservableCollection<Topping> CurrentToppingMenu
        {
            get
            {
                // Removes the current Required Topping for the selected product from the view allowing the user to toggle choices
                if (
                    SelectedOrderItem != null
                    && SelectedOrderItem is Product product
                    && product.RequiredChoices.Count > 0
                    && product.Category == SelectedCategory 
                )
                {
                    var filteredByItem = _currentToppingMenu.Where(t =>
                        !t.Equals(product.RequiredChoices.First())
                    );
                    return new ObservableCollection<Topping>(filteredByItem);
                }
                else if (DisplayToppings)
                {
                    // If we don't have any items selected remove the default topping from the view
                    var filteredByDefault = _currentToppingMenu.Where(t =>
                        !t.Equals(_currentProductMenu.First().RequiredChoices.First())
                    );
                    return new ObservableCollection<Topping>(filteredByDefault);
                }
                return _currentToppingMenu;
            }
            set => SetProperty(ref _currentToppingMenu, value);
        }

        // Displays an error message if minimum spend for delivery not achieved
        private bool _isError;
        public bool IsError
        {
            get => _isError;
            set => SetProperty(ref _isError, value);
        }

        //------        SESSION/NAVIGATION        ------//        
        public ICommand CollectionCommand { get; }
        public ICommand LogoutCommand { get; }        

        //------        CONSTRUCTOR     ------//
        public CashierViewModel(            
            IProductRepository<Product> productRepo,
            IProductRepository<Topping> toppingRepo,
            IOrderRepository orderRepo,
            ISession session,
            ObservableCollection<Product> products
        )
        {            
            _productRepo = productRepo;
            _toppingRepo = toppingRepo;
            _orderRepo = orderRepo;
            _session = session;
            
            // We can pass products back from checkout this way
            _currentOrderItems = products;

            // Set with default values initially
            _selectedCategory = "";
            _currentToppingMenu = [];
            _currentProductMenu = [];

            IsDeliveryCommand = new RelayGenericCommand(Delivery);
            // Binds to menu buttons to allow the user to add items to the current order
            AddOrderItemCommand = new RelayCommand<MenuItemBase>(AddOrderItem);
            // Binds to the void button to remove menu items
            RemoveOrderItemCommand = new RelayGenericCommand(RemoveOrderItem);
            // Binds to the HalfAndHalf button
            HalfAndHalfCommand = new RelayGenericCommand(HalfAndHalf);
            // Triggers Checkout process
            CheckoutCommand = new RelayGenericCommand(Checkout);

            // This binds to the menu buttons allowing the user to change the view
            SelectCategoryCommand = new RelayCommand<string>(SelectCategory);
            SelectSizeCommand = new RelayCommand<string>(SelectSize);

            // This binds to the Collections button
            CollectionCommand = new RelayGenericCommand(Collection);
            // This binds to the logout button
            LogoutCommand = new RelayGenericCommand(Logout);

            // Default category when view is rendered
            SelectCategory("Deal");
        }

        //------        ORDER        ------//

        private void Delivery()
        {
            IsDelivery = !IsDelivery;
            IsError = false;
        }

        private void AddOrderItem(MenuItemBase? orderItem)
        {
            if (orderItem == null)
                return;


            if (_activeDealParent != null && orderItem is Product selectedProduct)
            {
                // If we have an active deal handle it seperately
                HandleDealSelection(selectedProduct);
                return;
            }

            // If we have any product that isn't a half and half pizza
            if (orderItem is Product product && !IsHalfAndHalf)
            {                
                Product newItem = (Product)product.Clone();

                if (newItem.Category == "Deal")
                {
                    // We need to handle deals and items contained in a deal differently
                    // We create a deal group ID to manage the items contained in a single instance of a deal
                    _currentDealGroupID++;
                    
                    newItem.ParentDealID = _currentDealGroupID;
                    // We assign a field to hold the active deal parent - when set menu item choices will be handled as deal item selections
                    _activeDealParent = newItem;

                    CurrentOrderItems.Add(newItem);
                    SelectedOrderItem = newItem;
                    
                    // We initialize our deal choices and add them to the current product menu
                    InitializeDealChoices(newItem);
                } 
                else
                {
                    // Regular items can just be added to our current order
                    CurrentOrderItems.Add(newItem);                    
                    SelectedOrderItem = newItem;
                }
                
                OnPropertyChanged(nameof(OrderTotal));
                return;
            } 
            // Selection for the first half
            else if (orderItem is Product firstHalf && firstHalf.Category == "Pizza" && _tempFirstHalf == null)
            {
                _tempFirstHalf = (Product)firstHalf.Clone();
                CurrentOrderItems.Add(_tempFirstHalf);
                SelectedOrderItem = _tempFirstHalf;
                OnPropertyChanged(nameof(OrderTotal));
                return;
            } 
            else if (
                orderItem is Product secondHalf 
                && _tempFirstHalf != null
                && secondHalf.Category == "Pizza" 
                && secondHalf.SizeName == _tempFirstHalf.SizeName
                && secondHalf.Name != _tempFirstHalf.Name // Check our halfs are unique
            )
            {
                Product primaryHalf;
                Product secondaryHalf;

                // We want the primary half to be the more expensive half as that is the
                // ID that will retrieve the price in the database
                if (_tempFirstHalf.Price > secondHalf.Price)
                {
                    primaryHalf = _tempFirstHalf;
                    secondaryHalf = secondHalf;
                }
                else
                {
                    secondaryHalf = _tempFirstHalf;
                    primaryHalf = secondHalf; 
                }

                    Product finalPizza = new Product
                    {
                        ID = primaryHalf.ID,
                        Name = $"{primaryHalf.Name} / {secondaryHalf.Name}",
                        SecondHalfID = secondaryHalf.ID,
                        Price = primaryHalf.Price,
                        RequiredChoices = primaryHalf.RequiredChoices,
                        Category = primaryHalf.Category,
                        Allergens = [.. primaryHalf.Allergens.Union(secondaryHalf.Allergens)],
                        Toppings = [],
                        SizeName = primaryHalf.SizeName,
                    };

                // We clone the object to create a unique instance and subscribe event handlers
                finalPizza = (Product)finalPizza.Clone();

                HalfAndHalf();
                
                CurrentOrderItems.Add(finalPizza);
                SelectedOrderItem = finalPizza;
                
                OnPropertyChanged(nameof(OrderTotal));
                return;
            }
            else if (orderItem is Product)
            {
                // If user selects half/half and changes size / category then we will reach this case
                // We remove the temp pizza, reset the state of IsHalfAndHalf and call the function again
                HalfAndHalf();
                AddOrderItem(orderItem);
            }                        

            if (orderItem is Topping topping)
            {
                if (SelectedOrderItem == null)
                    return;

                if (
                    SelectedOrderItem.Category.Equals("Pizza", StringComparison.OrdinalIgnoreCase)
                    || SelectedOrderItem.Category.Equals("Kebab", StringComparison.OrdinalIgnoreCase)
                )
                {
                    if (
                        _toppingRepo
                            .GetProductsByCategory(SelectedOrderItem.Category, SelectedOrderItem.SizeName)
                            .Contains(topping)
                    )
                    {
                        // Check if we are changing a required choice
                        if (topping.ChoiceRequired)
                        {
                            SelectedOrderItem.RequiredChoices[0] = topping;
                            UpdateMenu();
                        }
                        else if (!SelectedOrderItem.Toppings.Remove(topping))
                        {
                            SelectedOrderItem.Toppings.Add(topping);
                        }
                        OnPropertyChanged(nameof(OrderTotal));
                        return;
                    }
                }                                
            }
        }

        // When the user selects a deal item we initialize the set items and find the items we need the user to select
        private void InitializeDealChoices(Product dealParent)
        {
            Product? firstPlaceholder = null;

            foreach (Product requiredItem in dealParent.RequiredChoices.ToList())
            {
                // Create a new instance to edit
                Product componentItem = (Product)requiredItem.Clone();
                // Link to our parent deal item
                componentItem.ParentDealID = dealParent.ParentDealID;
            
                if (requiredItem.ID != 0)
                {
                    // If the user can not choose we just add to orderItems
                    if (componentItem.Category == "Pizza" || componentItem.Category == "Kebab")
                        FindDefaultChoices(componentItem); // We need to set the default base / bread
                    CurrentOrderItems.Add(componentItem);
                }
                else
                {
                    // We track our placeholder products and add to orderItems
                    componentItem.IsPlaceholder = true;
                    CurrentOrderItems.Add(componentItem);

                    if (firstPlaceholder == null)
                    {
                        // Set the starting point for getting user choices
                        firstPlaceholder = componentItem;
                    }
                }

                // Now we navigate to the relevant menu
                if (firstPlaceholder != null)
                {
                    SelectedOrderItem = firstPlaceholder;

                    DealNavigation(firstPlaceholder);                                            
                } 
                else
                {
                    // Deal is complete - no choice required
                    _activeDealParent = null;
                    SelectedOrderItem = dealParent;
                    SelectCategory("Deal");
                }
            }
            
        }

        // This function handles user choices for deal items
        private void HandleDealSelection(Product selectedProduct)
        {
            if (_activeDealParent == null)
                return;

            Product? placeholderToReplace = CurrentOrderItems
                .FirstOrDefault(p => p.ParentDealID == _activeDealParent.ParentDealID && p.IsPlaceholder);

            if (placeholderToReplace == null)
            {
                FinalizeDeal();
                return;
            }

            bool isValid = selectedProduct.Category == placeholderToReplace.Category
                           && placeholderToReplace.SizeName == placeholderToReplace.SizeName;

            if (isValid)
            {
                // We will break bindings if we replace with the new product so we just update the placeholder
                placeholderToReplace.ID = selectedProduct.ID;
                placeholderToReplace.Name = selectedProduct.Name;
                placeholderToReplace.Allergens = selectedProduct.Allergens;
                placeholderToReplace.RequiredChoices = selectedProduct.RequiredChoices;
                placeholderToReplace.IsPlaceholder = false;
                placeholderToReplace.InitializeDealMember();

                Product? nextPlaceholder = CurrentOrderItems
                    .FirstOrDefault(p => p.ParentDealID == _activeDealParent.ParentDealID && p.IsPlaceholder) ?? null;

                if (nextPlaceholder != null)
                {
                    // Highlight the next selection
                    SelectedOrderItem = nextPlaceholder;
                    // Navigate to the relevant menu category and filter products
                    DealNavigation(nextPlaceholder);                    
                }
                else
                {
                    // If we don't have any more choices we can end the deal item selection flow
                    FinalizeDeal();
                }

                OnPropertyChanged(OrderTotal);
            }
            else
            {
                DealNavigation(placeholderToReplace);
            }
        }

        // This is a helper function to redirect the user to the correcet menu for the next deal choice required
        private void DealNavigation(Product itemToChoose)
        {
            SelectCategory(itemToChoose.Category);
            if (itemToChoose.Category == "Pizza")
            {
                SelectSize(itemToChoose.SizeName);
            } else
            {
                FilterMenuBySize(itemToChoose.SizeName);
            }

        }

        // Helper function for deal items - we remove any items in the category menu that don't match the required size
        private void FilterMenuBySize(string? size)
        {
            if (size == null)
                return;

            CurrentProductMenu = new ObservableCollection<Product>(CurrentProductMenu.ToList().Where(p => p.SizeName == size));
        }


        // This function resets our active deal and selects the parent item in the menu
        private void FinalizeDeal()
        {
            if (_activeDealParent == null)
                return;

            Product finalParent = _activeDealParent;
            _activeDealParent = null;

            SelectedOrderItem = finalParent;
            SelectCategory("Deal");
        }


        // This function removes all deal items when a single deal item is deleted
        private void CleanupDeal(int dealGroupID)
        {
            List<Product> productsToRemove = CurrentOrderItems
                .Where(p => p.ParentDealID == dealGroupID)
                .ToList();

            foreach (Product product in productsToRemove)
            {
                CurrentOrderItems.Remove(product);
            }

            if (_activeDealParent != null && _activeDealParent.ParentDealID == dealGroupID)
            {
                _activeDealParent = null;
                SelectedOrderItem = null;
                if (CurrentOrderItems.Count > 0)
                    SelectedOrderItem = CurrentOrderItems.Last();
                SelectCategory("Deal");                
            }
        }
        private void RemoveOrderItem()
        {
            if (SelectedOrderItem == null)
                return;

            if (SelectedOrderItem.ParentDealID.HasValue)
            {
                CleanupDeal(SelectedOrderItem.ParentDealID.Value);
                OnPropertyChanged(nameof(OrderTotal));
                return;
            }
            
            if (CurrentOrderItems.Remove(SelectedOrderItem))
            {
                OnPropertyChanged(nameof(OrderTotal));

                if (CurrentOrderItems.Count == 0)
                {
                    SelectedOrderItem = null;
                }
                else
                {
                    SelectedOrderItem = CurrentOrderItems.Last();
                }
            }                                                    
        }

        // Toggle function for our half and half selection
        private void HalfAndHalf()
        {
            IsHalfAndHalf = !IsHalfAndHalf;
            
            if (!IsHalfAndHalf && _tempFirstHalf != null)
            {
                // Reset our temporary field
                CurrentOrderItems.Remove(_tempFirstHalf);
                _tempFirstHalf = null;
            }
        }      

        

        private void Checkout()
        {
            if (_activeDealParent != null && _activeDealParent.ParentDealID.HasValue)
            {
                // We don't want to proceed to checkout if we have an unfinished deal!
                CleanupDeal(_activeDealParent.ParentDealID.Value);
            }

            if (CurrentOrderItems.Count > 0 && _session.CurrentUser != null)
            {
                if (IsDelivery && _totalCost < (decimal)12.00) 
                {
                    IsError = true;  
                    return;
                }

                _currentOrder = new Order{
                    UserID = _session.CurrentUser.Id, 
                    OrderProducts = CurrentOrderItems,
                    OrderStatus = "New",
                    OrderType = IsDelivery ? "Delivery" : "Collection"
                };
                

                _currentOrder.ID = _orderRepo.CreateOrder(_currentOrder);

                OnNavigate();             

                if (_currentOrder.ID != 0)
                {
                    EventLogger.LogInfo("Order created successfully!");
                }
                else
                {
                    EventLogger.LogError("Order creation failed..");
                }
            }
        }

        //------        MENU        ------//
        // Because we have the size Sub-Menu in pizzas - we need to seperate the menu rendering from category selection
        private void UpdateMenu()
        {
            List<Product> products = [];
            List<Topping> toppings = [];

            switch (SelectedCategory)
            {
                case "Deal":
                    products.AddRange(_productRepo.GetMealDeals());
                    break;
                case "Burger":
                    products.AddRange(_productRepo.GetProductsByCategory("Wrap"));
                    products.AddRange(_productRepo.GetProductsByCategory(SelectedCategory));
                    break;
                case "Side":
                    products.AddRange(_productRepo.GetProductsByCategory(SelectedCategory));
                    products.AddRange(_productRepo.GetProductsByCategory("Dip"));
                    break;
                case "Pizza":
                    products.AddRange(
                        _productRepo.GetProductsByCategory(SelectedCategory, CurrentSizeSelection)
                    );
                    toppings.AddRange(
                        _toppingRepo.GetProductsByCategory(SelectedCategory, CurrentSizeSelection)
                    );

                    // Here we set the default base for the pizza
                    products.ForEach(p =>
                    {
                        Topping? defaultBase = toppings.Find(t =>
                            t.ChoiceRequired
                            && t.Name.Equals("Tomato", StringComparison.OrdinalIgnoreCase)
                        );
                        Topping? bbqBase = toppings.Find(t =>
                            t.ChoiceRequired
                            && t.Name.Equals("BBQ", StringComparison.OrdinalIgnoreCase)
                        );

                        switch (p.Name.ToLower())
                        {
                            case "bbq chicken":
                                if (bbqBase != null)
                                    p.RequiredChoices.Add(bbqBase);
                                break;
                            default:
                                if (defaultBase != null)
                                    p.RequiredChoices.Add(defaultBase);
                                break;
                        }
                    });
                    break;
                case "Kebab":
                    products.AddRange(_productRepo.GetProductsByCategory(SelectedCategory));
                    toppings.AddRange(_toppingRepo.GetProductsByCategory(SelectedCategory));

                    // Make Pitta the default for kebabs
                    products.ForEach(p =>
                    {
                        Topping? defaultBread = toppings.Find(t =>
                            t.ChoiceRequired
                            && t.Name.Equals("Pitta", StringComparison.OrdinalIgnoreCase)
                        );

                        if (defaultBread != null)
                            p.RequiredChoices.Add(defaultBread);
                    });
                    break;
                default:
                    products.AddRange(_productRepo.GetProductsByCategory(SelectedCategory));
                    break;
            }

            CurrentProductMenu = new ObservableCollection<Product>(products);

            if (toppings.Count > 0)
            {
                CurrentToppingMenu = new ObservableCollection<Topping>(toppings);
            }
        }

        private void FindDefaultChoices(Product product)
        {
            List<Topping> toppings = [.. _toppingRepo.GetProductsByCategory(product.Category, product.SizeName).Where(t => t.ChoiceRequired)];
            
            Topping? defaultChoice = null;


            if (product.Category == "Pizza")
            {
                defaultChoice = product.Name == "BBQ Chicken" ? toppings.Find(t => t.Name == "BBQ") : toppings.Find(t => t.Name == "Tomato");
            } else if (product.Category == "Kebab")
            {
                defaultChoice = toppings.Find(t => t.Name == "Pitta");
            }
            else
            {
                return;
            }

            if (defaultChoice != null)
                product.RequiredChoices.Add(defaultChoice);
        }

        private void SelectCategory(string? category)
        {
            // Don't do anything if user selects the current category
            if (category == null || category == SelectedCategory)
                return;

            // We set the default value for the Pizza menu layout
            IsPizza = false;            

            // Assign the new category - Because we group some categories we need to handle conversion
            switch (category)
            {
                case "Pizza":
                    IsPizza = true;                    
                    CurrentSizeSelection = "Small";
                    break;
                case "Wrap":
                    category = "Burger";
                    break;
                case "Dip":
                    category = "Side";
                    break;
            }
            
            // Assign the category selected
            SelectedCategory = category;

            // Update the view
            UpdateMenu();
        }

        // Method to change the size selection for pizzas
        private void SelectSize(string? size)
        {
            if (size != null || CurrentSizeSelection != size)
            {                
                CurrentSizeSelection = size;
                UpdateMenu();
            }
        }

        //------        SESSION/NAVIGATION        -------//
        private void Collection() => OnNavigateBack();        
        private void Logout() => _session.Logout();                    
    }
}
