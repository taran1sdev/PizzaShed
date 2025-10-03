using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaShed.Model
{
    public class Topping : MenuItemBase
    {                               
        public bool ChoiceRequired { get; set; } = false;        

        public string MenuName => Name;
    }
}
