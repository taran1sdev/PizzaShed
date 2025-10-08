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
        public int ID { get; set; } = 0;

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

        // Returns the total price for the order - taking discount's into consideration
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

        // For displaying VAT amounts on receipts for compliance
        public string VAT => $"{TotalPrice * (decimal)0.2:N2}";                 

        public ObservableCollection<Product> OrderProducts { get; set; }

        public Promotion? Promo { get; set; }

        // When we initially create an order all we need is the userID and products in the order
        // This may need to change when we are creating orders for display but for now it's fine
        public Order(int userID, ObservableCollection<Product> products)
        {
            UserID = userID;
            OrderProducts = products;
        }
    }
}
