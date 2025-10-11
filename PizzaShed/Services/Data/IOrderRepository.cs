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
        
        public int CreateOrder(Order products);
        public bool UpdateDeliveryOrder(int orderId, int customerId, int distance);

        public bool UpdatePaidOrder(Order order);

        public Order? GetOrderByOrderNumber(int orderNumber);

        public ObservableCollection<Order> GetOrdersByRole(string role);

        public bool CreatePayment(int orderId, decimal amount, string paymentType);

        public bool DeleteOrder(int orderId);

        public (bool, ObservableCollection<string>) GetCollectionTimes();

        public (bool, string) GetDeliveryTime();
    }
}
