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

        public required decimal Price { get; set; }

        public string[] Allergens { get; set; } = [];
    }
}
