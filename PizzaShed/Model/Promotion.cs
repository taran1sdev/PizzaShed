using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaShed.Model
{
    public class Promotion
    {
        public int ID { get; set; } 
        
        public string? PromoCode { get; set;  }        

        public required string Description { get; set; }

        public decimal DiscountValue { get; set; }
        
        public string ReceiptName
        {
            get
            {
                if (PromoCode == null)
                {
                    return Description;
                }
                return $"{PromoCode}: {Description}"; 
            }
        }

        public string MenuName
        {
            get
            {
                if (PromoCode == null)
                {
                    return Description.Split(' ')[0];
                }
                return PromoCode;
            }
        }
    }
}
