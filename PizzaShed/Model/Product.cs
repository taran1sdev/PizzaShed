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

        // Helper function for products that are members of a deal
        public void InitializeDealMember()
        {
            SetupEventHandlers();
            OnPropertyChanged(nameof(ReceiptName));
        }      

        private void SetupEventHandlers()
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

        public override object Clone()
        {
            Product newProduct = (Product)this.MemberwiseClone();


            // Only deep clone non-deal items
            if (newProduct.Category != "Deal")
            {
                newProduct.RequiredChoices =
                new ObservableCollection<MenuItemBase>(
                    this.RequiredChoices.Select(r => (MenuItemBase)r.Clone())
                );

                newProduct.Toppings =
                    new ObservableCollection<Topping>(
                        this.Toppings.Select(t => (Topping)t.Clone())
                        );
            }
            

            newProduct.SetupEventHandlers();

            return newProduct;
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return RuntimeHelpers.GetHashCode(this);
        }

        public Product()
        {
            SetupEventHandlers();
        }

        public int? SecondHalfID;

        public int? ParentDealID = null;
        
        public bool IsPlaceholder = false;

        // This is for when we need to query toppings on existing orders
        public int OrderProductId = 0;

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
                    "drink" => $"{Name} ({SizeName})" +
                               $"\n\t\t\t\t\t\t£{Price:N2}",
                    "side" => $"{Name} ({SizeName})" +
                              $"\n\t\t\t\t\t\t£{Price:N2}" +
                              $"{(Allergens.Count > 0 ? $"\nContains: {string.Join(", ", Allergens)}" : "")}",
                    _ => $"{Name}" +
                         $"\n\t\t\t\t\t\t£{Price:N2}" +
                         $"{(Allergens.Count > 0 ? $"\nContains: {string.Join(", ", Allergens)}" : "")}",
                };
            }
        }
    }
}
