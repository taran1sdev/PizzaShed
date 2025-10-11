using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PizzaShed.Model
{
    public class Order : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public int ID { get; set; } = 0;

        public int UserID { get; set; }

        public int? CustomerID { get; set; }

        public string? OrderSource { get; set; }

        public string? OrderNotes { get; set; }

        public string? OrderType { get; set; }

        public required string OrderStatus { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime? CollectionTime { get; set; }

        public int? DriverID { get; set; }

        public bool Paid = false;        

        public decimal? DeliveryFee { get; set; }

        // Returns the total price for the order - taking discount's into consideration
        public decimal TotalPrice
        {
            get
            {
                decimal total = OrderProducts
                                .Sum(static p => p.Price + p.Toppings
                                .Sum(static t => t.Price));
                
                if (Promo != null) 
                    total -= (total * Promo.DiscountValue);

                // Add the delivery fee after calculating discounts
                if (DeliveryFee != null)
                    total += (decimal)DeliveryFee;

                // We have to round here - promotion calculation results in a tiny fraction
                // that breaks payment logic
                return Math.Round(total, 2);
            }
        }

        public decimal PriceAfterPayments
        {
            get
            {
                decimal total = TotalPrice;

                // Check first for existing keys
                foreach (string key in Payments.Keys)
                {
                    // Subtract the payment values from the displayed total
                    Payments[key].ForEach(p => total -= p);
                }
                
                
                return total;
            }
        }

        // Return the total price excluding meal deal items (and their toppings)
        public decimal PriceExcludingDeals => OrderProducts.Where(p => p.ParentDealID == null)
                                                           .Sum(static p => p.Price + p.Toppings
                                                           .Sum(static t => t.Price));                
            
        

        // For displaying VAT amounts on receipts for compliance
        public decimal VAT => TotalPrice * (decimal)0.2;

        public ObservableCollection<Product> OrderProducts { get; set; } = [];
        
        public Promotion? Promo { get; set; }

        // We use a dictionary to track payments that are made so we can support split payments
        public Dictionary<string, List<decimal>> Payments { get; set; } = [];
    }
}
