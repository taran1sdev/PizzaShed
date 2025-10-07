using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaShed.Model
{
    public class Order
    {
        public int? ID { get; }

        public int UserID { get; }

        public int? CustomerID { get; set; }

        public string OrderSource { get; set; }

        public string? OrderNotes { get; set; }

        public string OrderType { get; set; }

        public string OrderStatus { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime? CollectionTime { get; set; }

        public int? DriverID { get; set; }

        public bool Paid = false;

        public string? PaymentType { get; set; }

        public decimal TotalPrice
        {
            get
            {
                decimal total = OrderProducts.Sum(static p => p.Price + p.Toppings.Sum(static t => t.Price));
                if (Promo != null)
                {
                    total = total - (total * Promo.DiscountValue);
                }
                return total;
            }
        }

        public string VAT => $"{TotalPrice * (decimal)0.2:N2}";                 

        public ObservableCollection<Product> OrderProducts { get; set; }

        public Promotion? Promo { get; set; }

        public Order(int userID, ObservableCollection<Product> products)
        {
            UserID = userID;
            OrderProducts = products;
        }
    }
}
