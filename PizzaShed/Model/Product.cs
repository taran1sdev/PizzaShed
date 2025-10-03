using Microsoft.Xaml.Behaviors.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace PizzaShed.Model
{
    public class Product : MenuItemBase
    {                
        public required string Category { get; set; }

        public string? SizeName { get; set; }        
        
        // This List will hold choices required by the user
        // If the Product is a Deal - this will hold the Menu Items required        
        public List<MenuItemBase> RequiredChoices { get; set; } = [];
         
        public List<Topping> SelectedChoices { get; set; } = [];
        
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
                        $"\n{RequiredChoices.FirstOrDefault()?.DisplayName}" +
                        $"\n{(string.Join("\n", SelectedChoices.Select(t => $"{t.DisplayName}")))}" +
                        $"\nContains: {string.Join(", ", Allergens.Union(SelectedChoices.SelectMany(t => t.Allergens)))}",
                    "drink" => $"{Name} ({SizeName})",
                    "side" => $"{Name} ({SizeName})" +
                        $"\nContains: {(string.Join(", ", Allergens))}",
                    _ => $"{Name}",
                };
            }
        }
    }
}
