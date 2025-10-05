using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PizzaShed.Model;

namespace PizzaShed.Services.Data
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IDatabaseManager _databaseManager;
        public OrderRepository(IDatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        public int GetOrderNumber()
        {
            return 0;
        }


        public int CreateDeliveryOrder(ObservableCollection<Product> products, Customer customer)
        {
            return 0;
        }

        public int CreateCollectionOrder(ObservableCollection<Product> products, DateTime? collectionTime)
        {
            return 0;
        }

        public ObservableCollection<Product> GetOrderByOrderNumber(int orderNumber)
        {
            return [];
        }

        public ObservableCollection<Product> GetOrderItemsByRole(string role)
        {
            return [];
        }
    }
}
