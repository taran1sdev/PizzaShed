using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PizzaShed.Model;

namespace PizzaShed.Services.Data
{
    public interface IOrderRepository
    {
        public ObservableCollection<Promotion> FetchEligiblePromotions(decimal orderPrice);

        public int CreateDeliveryOrder(Order order, Customer customer);
        
        public int CreateCollectionOrder(Order products);        

        public Order? GetOrderByOrderNumber(int orderNumber);

        public ObservableCollection<Order> GetOrdersByRole(string role);

        public bool DeleteOrder(int orderId);
    }
}
