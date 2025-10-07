using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaShed.Model
{
    public class Promotion
    {
        public int ID { get; } 
        
        public string? PromoCode { get; }

        public string Name { get; }

        public decimal DiscountValue { get; }

        public decimal MinSpend { get; }
    }
}
