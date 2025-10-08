using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PizzaShed.Model;
using PizzaShed.Services.Logging;
using Microsoft.Data.SqlClient;
using System.Data;

namespace PizzaShed.Services.Data
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IDatabaseManager _databaseManager;
        public OrderRepository(IDatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }


        public int CreateDeliveryOrder(Order order, Customer customer)
        {
            return 0;
        }

        // We create a stored procedure in MSSQL to handle order creation
        // this allows us to create tables containing orders / toppings
        // and insert orders in a single DB transaction
        private static (DataTable products, DataTable toppings) CreateTVPs(List<Product> products)
        {
            DataTable productTable = new DataTable();
            productTable.Columns.Add("product_id", typeof(int));
            productTable.Columns.Add("size_name", typeof(string));
            productTable.Columns.Add("deal_id", typeof(int));
            productTable.Columns.Add("deal_instance_id", typeof(int));
            productTable.Columns.Add("client_product_id", typeof(int));


            DataTable toppingTable = new DataTable();
            toppingTable.Columns.Add("client_product_id", typeof(int));
            toppingTable.Columns.Add("topping_id", typeof(int));

            int clientCounter = 1;

            foreach (Product product in products)
            {
                if (product.Category == "Deal")
                {
                    productTable.Rows.Add(
                        (object)DBNull.Value,
                        (object)DBNull.Value,
                        product.ID,
                        product.ParentDealID,
                        clientCounter
                    );
                } 
                else
                {
                    productTable.Rows.Add(
                        product.ID,
                        product.SizeName,
                        (object)DBNull.Value,
                        product.ParentDealID ?? (object)DBNull.Value,
                        clientCounter
                    );                    
                }
                
                if (product.Category == "Pizza" || product.Category == "Kebab")
                {
                    toppingTable.Rows.Add(
                        clientCounter,
                        product.RequiredChoices[0].ID
                    );
                }

                foreach (Topping topping in product.Toppings)
                {
                    toppingTable.Rows.Add(
                        clientCounter,
                        topping.ID
                    );
                }
                clientCounter++;
            }
            
            return (productTable, toppingTable);
        }

        public int CreateCollectionOrder(Order order)
        {
            var (productTable, toppingTable) = CreateTVPs(order.OrderProducts.ToList());

            string storedProcedure = "CreateCollectionOrder";

            try
            {
                return _databaseManager.ExecuteQuery(conn =>
                {
                    using (SqlCommand query = new SqlCommand(storedProcedure, conn))
                    {
                        query.CommandType = CommandType.StoredProcedure;

                        query.Parameters.AddWithValue("@userID", order.UserID);
                        query.Parameters.AddWithValue("@price", order.TotalPrice);

                        SqlParameter productListParameter = new SqlParameter("@ProductList", productTable)
                        {
                            SqlDbType = SqlDbType.Structured,
                            TypeName = "ProductListType"
                        };

                        SqlParameter toppingListParameter = new SqlParameter("@ToppingList", toppingTable)
                        {
                            SqlDbType = SqlDbType.Structured,
                            TypeName = "ToppingListType"
                        };

                        query.Parameters.Add(productListParameter);
                        query.Parameters.Add(toppingListParameter);
                        
                        
                        using (SqlDataReader reader = query.ExecuteReader())
                        { 
                            if (reader.HasRows && reader.Read())
                            {
                                int orderId = reader.IsDBNull(reader.GetOrdinal("NewOrderID")) ? 0 : Convert.ToInt32(reader["NewOrderID"]);
                                return orderId;
                            }
                            return 0;
                        }
                    }
                });                
            }
            catch (Exception ex)
            {
                EventLogger.LogError("Failed to create new collection order: " + ex.Message);
            }

            return 0;
        }

        public Order GetOrderByOrderNumber(int orderNumber)
        {
            return null;
        }

        public ObservableCollection<Order> GetOrdersByRole(string role)
        {
            return null;
        }
    }
}
