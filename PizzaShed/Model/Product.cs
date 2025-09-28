using Microsoft.Xaml.Behaviors.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaShed.Model
{
    public class Product
    {
        public int ProductId { get; set; }

        public required string Name { get; set; }

        public required string Category { get; set; }

        public required string SizeName { get; set; }

        public decimal Price { get; set; }                
        
        public int? ToppingId { get; set; }

        public required string[] Allergens { get; set; }

        public string DisplayName
        {
            get
            {
                // Toppings will show as indented
                if (ToppingId.HasValue)
                {
                    return $"- {Name}";
                }

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
