using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaShed.Model
{
    // This is a base class for all items on the menu - Products and Toppings
    public class MenuItemBase
    {
        public int ID { get; set; }

        public required string Name { get; set; }

        public required decimal? Price { get; set; }

        // HashSet makes sure we don't have duplicate values
        public HashSet<string> Allergens { get; set; } = [];

        public string DisplayName => $" - {Name}";
    }
}
