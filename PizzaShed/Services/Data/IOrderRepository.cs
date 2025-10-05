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
        public int GetOrderNumber();

        public int CreateDeliveryOrder(ObservableCollection<Product> products, Customer customer);
        
        public int CreateCollectionOrder(ObservableCollection<Product> products, DateTime? collectionTime);

        public ObservableCollection<Product> GetOrderByOrderNumber(int orderNumber);

        public ObservableCollection<Product> GetOrderItemsByRole(string role);
    }
}
