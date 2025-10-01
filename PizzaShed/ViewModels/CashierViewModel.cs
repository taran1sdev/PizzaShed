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

namespace PizzaShed.ViewModels
{
    public class CashierViewModel : ViewModelBase
    {
        private readonly IProductRepository<Product> _productRepo;
        private readonly IProductRepository<Topping> _toppingRepo;
        private readonly ISession _session;
        private ObservableCollection<Product> _currentOrderItems;

        public ObservableCollection<Product> CurrentOrderItems
        {
            get => _currentOrderItems;
            set => SetProperty(ref _currentOrderItems, value);
        }

        private ObservableCollection<Product> _currentProductMenu;
        public ObservableCollection<Product> CurrentProductMenu 
        {
            get => _currentProductMenu;
            set => SetProperty(ref _currentProductMenu, value);
        }

        // Property for logic/styling when displaying toppings
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

        // Property to hold our available toppings
        private ObservableCollection<Topping> _currentToppingMenu;
        public ObservableCollection<Topping> CurrentToppingMenu
        {
            get 
            {                
                int? idToExclude = null;                
                // Removes the current Required Topping for the selected product from the view allowing the user to toggle choices
                if(SelectedOrderItem != null && SelectedOrderItem.RequiredChoices.Count > 0)
                {
                    idToExclude = SelectedOrderItem?.RequiredChoices?.FirstOrDefault()?.ID;
                    
                    var filteredByItem = _currentToppingMenu.Where(t => t.ID != idToExclude);
                    return new ObservableCollection<Topping>(filteredByItem);
                } else if (DisplayToppings)
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

        // Property to hold the order item the user has selected
        private Product? _selectedOrderItem;
        public Product? SelectedOrderItem
        {
            get => _selectedOrderItem;
            set
            {
                SetProperty(ref _selectedOrderItem, value);
                SetProperty(ref _selectedToppingItem, null);
            }
        }


        // Property to hold the topping selected by the user
        private Topping? _selectedToppingItem;
        public Topping? SelectedToppingItem
        {
            get => _selectedToppingItem;
            set 
            {
                // If a topping is selected we unset the order item
                SetProperty(ref _selectedToppingItem, value);
                SetProperty(ref _selectedOrderItem, null);
            }
        }

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

        // Property that toggles the size selection and 50/50 buttons for Pizzas
        private bool _isPizza;
        public bool IsPizza
        {
            get => _isPizza;
            set => SetProperty(ref _isPizza, value);
        }

        // Property to hold the size menu selected by the user when displayed
        private string? _currentSizeSelection;
        public string? CurrentSizeSelection
        {
            get => _currentSizeSelection;
            set => SetProperty(ref _currentSizeSelection, value);
        }

        public ICommand SelectCategoryCommand { get; }
        public ICommand SelectSizeCommand { get; }

        public CashierViewModel(IProductRepository<Product> productRepo, IProductRepository<Topping> toppingRepo, ISession session)
        {
            _productRepo = productRepo;
            _toppingRepo = toppingRepo;
            _session = session;

            _currentOrderItems = [];                     

            // This binds the Select category function to our buttons
            SelectCategoryCommand = new RelayCommand<string>(SelectCategory);
            SelectSizeCommand = new RelayCommand<string>(SelectSize);

            // Default category when view is rendered
            SelectCategory("Deals");
        }

        private void SelectCategory(string? category)
        {
            // Don't do anything if user selects the current category
            if (category == null || category == SelectedCategory) return;

            // Assign the new category - this will trigger the property changed event and update the view
            SelectedCategory = category;

            if (category != "Pizza")
            {
                IsPizza = false;
                CurrentSizeSelection = null;
            } else
            {
                IsPizza = true;
                CurrentSizeSelection = "Small";
            }

            List<Product> products = [];
            List<Topping> toppings = [];

            switch(category)
            {
                case "Deals":
                    // Implement deals as menu items
                    break;
                case "Burger":
                    products.AddRange(_productRepo.GetProductsByCategory("Wrap"));
                    products.AddRange(_productRepo.GetProductsByCategory(category));
                    break;
                case "Side":
                    products.AddRange(_productRepo.GetProductsByCategory(category));
                    products.AddRange(_productRepo.GetProductsByCategory("Dip"));
                    break;
                case "Pizza":
                    products.AddRange(_productRepo.GetProductsByCategory(category, CurrentSizeSelection));
                    toppings.AddRange(_toppingRepo.GetProductsByCategory(category, CurrentSizeSelection));
                    
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
                    products.AddRange(_productRepo.GetProductsByCategory(category));
                    toppings.AddRange(_toppingRepo.GetProductsByCategory(category));

                    // Make Pitta the default for kebabs
                    products.ForEach(p =>
                    {
                        Topping? defaultBread = toppings.Find(t => t.ChoiceRequired && t.Name.Equals("Pitta", StringComparison.OrdinalIgnoreCase));

                        if (defaultBread != null) p.RequiredChoices.Add(defaultBread);
                    });
                    break;
                default:
                    products.AddRange(_productRepo.GetProductsByCategory(category));
                    break;
            }

            CurrentProductMenu = new ObservableCollection<Product>(products);
            
            if(toppings.Count > 0)
            {                
                CurrentToppingMenu = new ObservableCollection<Topping>(toppings);                
            }
            
        }

        private void SelectSize(string? size)
        {
            if (size != null || CurrentSizeSelection != size)
            {
                CurrentSizeSelection = size;                
            }
        }
    }
}
