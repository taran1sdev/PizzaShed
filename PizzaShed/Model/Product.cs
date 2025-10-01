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

        public required string SizeName { get; set; }        
        
        // This List will hold choices required by the user
        // If the Product is a Deal - this will hold the Menu Items required        
        public List<Topping> RequiredChoices { get; set; } = [];

        public List<Topping> SelectedChoices { get; set; } = [];
        
        // We might be able to use SelectedChoices for this.
        //public List<Topping> ToppingSelection { get; set; } = [];
        public string MenuName
        {
            get
            {
                return Category.ToLower() switch
                {
                    "pizza" or "burger" or "wrap" => Name,                    
                    _ => $"{Name}\n({SizeName})",                
                };
            }
        }
        
        public string RecieptName
        {
            get
            {
                // Return the value that will be displayed on order info / recipts
                // If applicable display the size, toppings and allergens in the product
                return Category.ToLower() switch
                {
                    "pizza" or "kebab" => $"{Name} ({SizeName[1..].ToUpper()})" +
                        $"\n{RequiredChoices.FirstOrDefault()}" +
                        $"\n{(string.Join("\n", SelectedChoices.Select(t => $"{t.DisplayName}")))}" +
                        $"\nContains: {(string.Join(", ", Allergens.Concat(SelectedChoices.Select(t => $"{t.Allergens}"))).Distinct())}",
                    "drink" => $"{Name} ({SizeName})",
                    "side" => $"{Name} ({SizeName})" +
                        $"\nContains: {(string.Join(", ", Allergens))}",
                    _ => $"{Name}",
                };
            }
        }
    }
}
