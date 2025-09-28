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

namespace PizzaShed.ViewModels
{
    public class CashierViewModel : ViewModelBase
    {
        private readonly IProductRepository<Product> _productRepo;
        private readonly IProductRepository<Topping> _topping_Repo;

        private ObservableCollection<Product> _currentOrderItems;
        private ObservableCollection<MenuItemBase> _currentProductMenu;

        public ObservableCollection<Product> CurrentProductMenu { get; set; }
        
        private MenuItemBase _selectedOrderItem;
        private string _selectedCategory;

        public MenuItemBase SelectedOrderItem
        {
            get => _selectedOrderItem;
            set => SetProperty(ref _selectedOrderItem, value);
        }
        public string SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }


        public ICommand SelectCategoryCommand { get; }

        public CashierViewModel(IProductRepository<Product> productRepo, IProductRepository<Topping> toppingRepo)
        {
            _productRepo = productRepo;
            _topping_Repo = toppingRepo;

            _currentOrderItems = [];
            //_currentProductMenu = //function with default values            

            SelectCategoryCommand = new RelayCommand<string>(SelectCategory);

            SelectCategory("Deals");
        }

        private void SelectCategory(string category)
        {
            if (category == null || category == SelectedCategory) return;

            SelectedCategory = category;

            List<Product> products = _productRepo.GetProductsByCategory(category);

            CurrentProductMenu = new ObservableCollection<Product>(products);            
        }
    }
}
