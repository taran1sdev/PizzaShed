using PizzaShed.Commands;
using System;

namespace PizzaShed.Model
{
    public class Topping : MenuItemBase
    {       
        public bool ChoiceRequired { get; set; } = false;

        public string? ToppingType { get; set; } = null;

        public string MenuName => Name;
    }
}
