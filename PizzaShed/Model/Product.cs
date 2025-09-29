using Microsoft.Xaml.Behaviors.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaShed.Model
{
    public class Product : MenuItemBase
    {                
        public required string Category { get; set; }

        public required string SizeName { get; set; }        
        
        // This List will hold choices required by the user
        // If the Product is a Deal - this will hold the Menu Items required        
        public List<MenuItemBase> RequiredChoices { get; set; } = [];

        public List<MenuItemBase> SelectedChoices { get; set; } = [];
        
        // We might be able to use SelectedChoices for this.
        //public List<Topping> ToppingSelection { get; set; } = [];
        public string MenuName
        {
            get
            {
                return Category.ToLower() switch
                {
                    "pizza" => Name,
                    _ => $"{Name}\n({SizeName})",
                };
            }
        }
        
        public string RecieptName
        {
            get
            {
                // Return the product name and size for display on POS / Order tickets
                return Category.ToLower() switch
                {
                    "pizza" or "kebab" => $"{Name} ({SizeName[1..].ToUpper()})",
                    "drink" or "side" => $"{Name} ({SizeName})",
                    _ => $"{Name}",
                };
            }
        }
    }
}
