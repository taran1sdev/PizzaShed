using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
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
        private readonly ISession _session;

        //------        ORDER       ------//
        public ICommand AddOrderItemCommand { get; }
        public ICommand AddToppingItemCommand { get; }
        public ICommand RemoveOrderItemCommand { get; }
        public ICommand RemoveToppingItemCommand { get; }

        private ObservableCollection<Product> _currentOrderItems;

        public ObservableCollection<Product> CurrentOrderItems
        {
            get => _currentOrderItems;
            set => SetProperty(ref _currentOrderItems, value);
        }

        // This property displays the total cost of the order to the user
        public string OrderTotal
        {
            get
            {
                decimal? totalCost = 0;

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
                return $"£{totalCost:N2}";
            }
        }

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

        // This is a fix for duplicate items in an order
        // ListView will always select the first instance of an object 
        // this way we force selection when item already exists in the order
        private int _selectedOrderIndex = -1;
        public int SelectedOrderIndex
        {
            get => _selectedOrderIndex;
            set => SetProperty(ref _selectedOrderIndex, value);
        }

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

        //------        SESSION        ------//
        public ICommand LogoutCommand { get; }

        //------        CONSTRUCTOR     ------//
        public CashierViewModel(
            IProductRepository<Product> productRepo,
            IProductRepository<Topping> toppingRepo,
            ISession session
        )
        {
            _productRepo = productRepo;
            _toppingRepo = toppingRepo;
            _session = session;

            _currentOrderItems = [];

            // Binds to menu buttons to allow the user to add items to the current order
            AddOrderItemCommand = new RelayCommand<MenuItemBase>(AddOrderItem);
            // Binds to the void button to remove menu items
            RemoveOrderItemCommand = new RelayGenericCommand(RemoveOrderItem);

            // This binds to the menu buttons allowing the user to change the view
            SelectCategoryCommand = new RelayCommand<string>(SelectCategory);
            SelectSizeCommand = new RelayCommand<string>(SelectSize);

            // This binds to the logout button
            LogoutCommand = new RelayGenericCommand(Logout);

            // Default category when view is rendered
            SelectCategory("Deals");
        }

        //------        ORDER        ------//
        private void AddOrderItem(MenuItemBase? orderItem)
        {
            if (orderItem == null)
                return;

            if (orderItem is Product product)
            {
                CurrentOrderItems.Add(product);
                // Select the item added
                SelectedOrderIndex = CurrentOrderItems.Count - 1; 
                OnPropertyChanged(nameof(OrderTotal));
                return;
            }

            if (SelectedOrderItem == null)
                return;
            
            if (orderItem is Topping topping)
            {                
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

        private void RemoveOrderItem()
        {
            if (SelectedOrderItem == null)
                return;

            if (CurrentOrderItems.Remove(SelectedOrderItem))
            {                
                OnPropertyChanged(nameof(OrderTotal));

                if (CurrentOrderItems.Count == 0)
                {
                    SelectedOrderItem = null;
                }
                else
                {
                    SelectedOrderIndex = CurrentOrderItems.Count - 1;
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
                case "Deals":
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

        //------        SESSION        -------//
        private void Logout()
        {
            if (_session.IsLoggedIn)
            {
                _session.Logout();
            }
        }
    }
}
