using PizzaShed.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaShed.Model
{
    // This is a base class for all items on the menu - Products and Toppings
    public class MenuItemBase : ICloneable
    {
        // We store this command in the object to simplify integration testing
        public RelayCommand<MenuItemBase> AddOrderItemCommand { get; set; } = default!;

        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }

        public int ID { get; set; }

        public required string Name { get; set; }

        public required decimal Price { get; set; }

        // We override these methods so we can compare our menu items
        public override bool Equals(object? obj)
        {
            if (obj is not MenuItemBase item)
            {
                return false;
            }

            // ID is not unique but (ID,Price) is always unique
            return item.ID == ID && item.Price == Price;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ID, Price);
        }

        // HashSet makes sure we don't have duplicate values
        public HashSet<string> Allergens { get; set; } = [];

        public string DisplayName => $"\n - {Name}";
    }
}
