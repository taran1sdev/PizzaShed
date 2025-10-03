using PizzaShed.Services.Data;
using PizzaShed.Model;
using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using PizzaShed.Commands;
using System.Configuration;
using System.Windows;
using PizzaShed.Services.Logging;
using System.Windows.Navigation;

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

        // Property to hold the order item the user has selected
        private MenuItemBase? _selectedOrderItem;
        public MenuItemBase? SelectedOrderItem
        {
            get => _selectedOrderItem;
            set
            {
                SetProperty(ref _selectedOrderItem, value);
            }
        }

        // Property to hold the topping selected by the user
        //private Topping? _selectedToppingItem;
        //public Topping? SelectedToppingItem
        //{
        //    get => _selectedToppingItem;
        //    set 
        //    {
        //        // If a topping is selected we unset the order item
        //        SetProperty(ref _selectedToppingItem, value);
        //        SetProperty(ref _selectedOrderItem, null);
        //    }
        //}

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
                    _ => false
                };
            }
        }

        // Property to hold our available toppings - filters the current/default required choice (Pizza Base / Kebab Bread)
        private ObservableCollection<Topping> _currentToppingMenu;
        public ObservableCollection<Topping> CurrentToppingMenu
        {
            get
            {
                int? idToExclude = null;
                // Removes the current Required Topping for the selected product from the view allowing the user to toggle choices
                if (SelectedOrderItem != null && SelectedOrderItem is Product product && product.RequiredChoices.Count > 0)
                {
                    idToExclude = product?.RequiredChoices?.FirstOrDefault()?.ID;

                    var filteredByItem = _currentToppingMenu.Where(t => t.ID != idToExclude);
                    return new ObservableCollection<Topping>(filteredByItem);
                }
                else if (DisplayToppings)
                {
                    // If we don't have any items selected remove the default topping from the view 
                    idToExclude = CurrentProductMenu?.FirstOrDefault()?.RequiredChoices?.FirstOrDefault()?.ID;

                    var filteredByDefault = _currentToppingMenu.Where(t => t.ID != idToExclude);
                    return new ObservableCollection<Topping>(filteredByDefault);
                }
                return _currentToppingMenu;
            }
            set => SetProperty(ref _currentToppingMenu, value);
        }

        //------        SESSION        ------//
        public ICommand LogoutCommand { get; }


        //------        CONSTRUCTOR     ------//
        public CashierViewModel(IProductRepository<Product> productRepo, IProductRepository<Topping> toppingRepo, ISession session)
        {
            _productRepo = productRepo;
            _toppingRepo = toppingRepo;
            _session = session;

            _currentOrderItems = [];

            // Binds to menu buttons to allow the user to add items to the current order
            AddOrderItemCommand = new RelayCommand<MenuItemBase>(AddOrderItem);
            // Binds to the void button to remove menu items
            RemoveOrderItemCommand = new RelayCommand<MenuItemBase>(RemoveOrderItem);                        

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
            if (orderItem == null) return;

            if (orderItem is Product product)
            {
                CurrentOrderItems.Add(product);
            }

            if (orderItem is Topping topping)
            {

            }
        }

        private void RemoveOrderItem(MenuItemBase? orderItem)
        {
            if (orderItem == null) return;

            if (orderItem is Product product)
            {
                
            }

            if (orderItem is Topping topping)
            {

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
                    products.AddRange(_productRepo.GetProductsByCategory(SelectedCategory, CurrentSizeSelection));
                    toppings.AddRange(_toppingRepo.GetProductsByCategory(SelectedCategory, CurrentSizeSelection));

                    // Here we set the default base for the pizza

                    products.ForEach(p => {

                        Topping? defaultBase = toppings.Find(t => t.ChoiceRequired && t.Name.Equals("Tomato", StringComparison.OrdinalIgnoreCase));
                        Topping? bbqBase = toppings.Find(t => t.ChoiceRequired && t.Name.Equals("BBQ", StringComparison.OrdinalIgnoreCase));

                        switch (p.Name.ToLower())
                        {
                            case "bbq chicken":
                                if (bbqBase != null) p.RequiredChoices.Add(bbqBase);
                                break;
                            default:
                                if (defaultBase != null) p.RequiredChoices.Add(defaultBase);
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
                        Topping? defaultBread = toppings.Find(t => t.ChoiceRequired && t.Name.Equals("Pitta", StringComparison.OrdinalIgnoreCase));

                        if (defaultBread != null) p.RequiredChoices.Add(defaultBread);
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
            if (category == null || category == SelectedCategory) return;

            // Assign the new category - this will trigger the property changed event and update the view
            SelectedCategory = category;

            if (category == "Pizza")
            {
                // Set the property to render the size selection buttons and set initial size to display
                IsPizza = true;
                CurrentSizeSelection = "Small";
            } else
            {
                IsPizza = false;       
                CurrentSizeSelection = null;
            }
            
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
