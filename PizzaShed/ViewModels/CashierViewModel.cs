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
        private readonly IProductRepository<Topping> _topping_Repo;

        private ObservableCollection<Product> _currentOrderItems;
        private ObservableCollection<Product> _currentProductMenu;
        
        private bool _isVisible;

        private string? _currentSizeSelection;

        public string? CurrentSizeSelection
        {
            get => _currentSizeSelection;
            set => SetProperty(ref _currentSizeSelection, value);
        }

            

        public ObservableCollection<Product> CurrentProductMenu 
        {
            get => _currentProductMenu;
            set => SetProperty(ref _currentProductMenu, value);
        }
        
        private Product _selectedOrderItem;

        private string _selectedCategory;

        public Product SelectedOrderItem
        {
            get => _selectedOrderItem;
            set => SetProperty(ref _selectedOrderItem, value);
        }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }


        public ICommand SelectCategoryCommand { get; }
        public ICommand SelectSizeCommand { get; }

        public CashierViewModel(IProductRepository<Product> productRepo, IProductRepository<Topping> toppingRepo)
        {
            _productRepo = productRepo;
            _topping_Repo = toppingRepo;

            _currentOrderItems = [];                     

            // This binds the Select category function to our buttons
            SelectCategoryCommand = new RelayCommand<string>(SelectCategory);
            SelectSizeCommand = new RelayCommand<string>(SelectSize);

            // Default category when view is rendered
            SelectCategory("Deals");
        }

        private void SelectCategory(string category)
        {
            // Don't do anything if user selects the current category
            if (category == null || category == SelectedCategory) return;

            // Assign the new category - this will trigger the property changed event and update the view
            SelectedCategory = category;

            if (category != "Pizza")
            {
                IsVisible = false;
                CurrentSizeSelection = null;
            } else
            {
                IsVisible = true;
                CurrentSizeSelection = "Small";
            }

                List<Product> products = [];

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
                    break;
                default:
                    products.AddRange(_productRepo.GetProductsByCategory(category));
                    break;
            }


            // Just for debugging
            foreach (Product product in products)
            {
                EventLogger.LogInfo($"{product.Name}");
            }

            CurrentProductMenu = new ObservableCollection<Product>(products);            
        }

        private void SelectSize(string size)
        {
            if (size != null || CurrentSizeSelection != size)
            {
                CurrentSizeSelection = size;                
            }
        }
    }
}
