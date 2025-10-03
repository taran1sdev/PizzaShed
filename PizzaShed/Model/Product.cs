using Microsoft.Xaml.Behaviors.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace PizzaShed.Model
{
    public class Product : MenuItemBase, INotifyPropertyChanged
    {
        // We create an OnPropertyChanged method to update our view when toppings are added or removed
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public Product()
        {
            Toppings.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(ReceiptName));
            };

            RequiredChoices.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(ReceiptName)); 
            };  
        }

        public required string Category { get; set; }

        public string? SizeName { get; set; }        
        
        // This collection will hold choices required by the user
        // If the Product is a Deal - this will hold the Menu Items required        
        public ObservableCollection<MenuItemBase> RequiredChoices { get; set; } = [];
         
        public ObservableCollection<Topping> Toppings { get; set; } = [];

        public string MenuName
        {
            get
            {
                return Category.ToLower() switch
                {
                    "pizza" or "burger" or "wrap" or "deal" => Name,                      
                    _ => $"{Name}\n({SizeName})",                
                };
            }
        }        

        public string ReceiptName
        {
            get
            {
                // Return the value that will be displayed on order info / recipts
                // If applicable display the size, toppings and allergens in the product
                return Category.ToLower() switch
                {
                    "pizza" or "kebab" => $"{Name} ({SizeName?[..1].ToUpper()})" +
                        $"\n{(Category.ToLower() == "pizza" ? "Base: " : "Bread: ")}{RequiredChoices.FirstOrDefault()?.Name}" +
                        $"{string.Join("", Toppings.SelectMany(t => t.DisplayName))}" + 
                        $"\n\t\t\t\t\t\t£{Price + Toppings.Sum(t => t.Price):N2}" +
                        $"\nContains: {string.Join(", ", Allergens.Union(Toppings.SelectMany(t => t.Allergens)))}",
                    "drink" => $"{Name} ({SizeName})",
                    "side" => $"{Name} ({SizeName})" +
                        $"\nContains: {(string.Join(", ", Allergens))}",
                    _ => $"{Name}",
                };
            }
        }
    }
}
