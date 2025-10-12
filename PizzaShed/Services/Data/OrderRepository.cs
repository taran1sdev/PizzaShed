using Microsoft.Data.SqlClient;
using PizzaShed.Model;
using PizzaShed.Services.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PizzaShed.Services.Data
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IDatabaseManager _databaseManager;
        public OrderRepository(IDatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        public ObservableCollection<Promotion> FetchEligiblePromotions(decimal orderPrice)
        {
            string queryString = @"
                SELECT 
	                promo_id, 
	                description,
                    promo_code,
	                discount_value,
	                min_spend
                FROM Promotions 
                WHERE min_spend <= @orderPrice;";

            try
            {
                return _databaseManager.ExecuteQuery(conn => 
                {
                    
                    using (SqlCommand query = new SqlCommand(queryString, conn))
                    {
                        query.Parameters.AddWithValue("@orderPrice", orderPrice);

                        using (SqlDataReader reader = query.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                ObservableCollection<Promotion> promotions = [];

                                while (reader.Read())
                                {
                                    int promoId = reader.IsDBNull(reader.GetOrdinal("promo_id")) ? 0 : Convert.ToInt32(reader["promo_id"]);
                                    string? description = reader.IsDBNull(reader.GetOrdinal("description")) ? string.Empty : reader["description"].ToString();
                                    string? promo_code = reader.IsDBNull(reader.GetOrdinal("promo_code")) ? null : reader["promo_code"].ToString();
                                    decimal discount = reader.IsDBNull(reader.GetOrdinal("discount_value")) ? 0 : Convert.ToDecimal(reader["discount_value"]);

                                    if (description != null)
                                    {
                                        promotions.Add(new Promotion
                                        {
                                            ID = promoId,
                                            Description = description,
                                            PromoCode = promo_code,
                                            DiscountValue = discount
                                        });

                                    }
                                }
                                return promotions;
                            }
                            return [];
                        }
                    }
                });                   
            }
            catch (Exception ex)
            {
                EventLogger.LogError("Failed to fetch promotions " + ex.Message);
            }
            return [];
        }

        // Function to update a delivery order once we have customer information
        public bool UpdateDeliveryOrder(int orderId, int customerId, int distance)
        {
            // This query creates a variable to hold the delivery fee then updates the 
            // record in the order table
            string queryString = @"
                DECLARE @delivery_fee smallmoney;
                
                SELECT @delivery_fee = price 
                FROM Delivery_Fees 
                WHERE max_distance >= @distance
                ORDER BY price DESC;
                                
                UPDATE Orders
                SET customer_id = @customerID,
                delivery_fee = @delivery_fee,
                total_price = total_price + @delivery_fee
                WHERE order_id = @orderID
                ";

            try
            {
                return _databaseManager.ExecuteQuery(conn =>
                {
                    using (SqlCommand query = new SqlCommand(queryString, conn))
                    {
                        query.Parameters.AddWithValue("@orderID", orderId);
                        query.Parameters.AddWithValue("@customerID", customerId);
                        query.Parameters.AddWithValue("@distance", distance);

                        if (query.ExecuteNonQuery() > 0)
                        {
                            EventLogger.LogInfo("Successfully updated order record");
                            return true;
                        }
                        return false;
                    }
                });
            }
            catch (Exception ex)
            {
                EventLogger.LogError("Failed to update order: " + ex.Message);
            }
            return false;
        }        

        // Function to update an order once payment has been made
        public bool UpdatePaidOrder(Order order)
        {            
            string queryString = @"
                UPDATE Orders
                SET order_status_id = (SELECT order_status_id FROM Order_Status WHERE status_name LIKE @status), 
                pizza_ready = @pizzaReady,
                grill_ready = @grillReady,
                collection_time = @collectionTime,
                order_source = @orderSource,
                order_notes = @orderNotes,                
                paid = @orderPaid,
                promo_id = @promoId,
                total_price = @totalPrice
                WHERE order_id = @orderId;";


            try
            {
                return _databaseManager.ExecuteQuery(conn =>
                {
                    using (SqlCommand query = new SqlCommand(queryString, conn))
                    {
                        // We add the parameters we definitely know
                        query.Parameters.AddWithValue("@orderId", order.ID);
                        query.Parameters.AddWithValue("@orderSource", order.OrderSource);
                        query.Parameters.AddWithValue("@totalPrice", order.TotalPrice);

                        if (order.OrderNotes == null)
                        {
                            query.Parameters.AddWithValue("@orderNotes", DBNull.Value);
                        } 
                        else
                        {
                            query.Parameters.AddWithValue("@orderNotes", order.OrderNotes);
                        }

                        // If the order has no pizzas or no grill items we use a different status
                        int pizzaCount;
                        int grillCount;

                        pizzaCount = order.OrderProducts
                                            .ToList()
                                            .Count(p => p.Category == "Pizza");
                        
                        grillCount = order.OrderProducts
                                            .ToList()
                                            .Count(p =>
                                            p.Category == "Kebab"
                                            || p.Category == "Side"
                                            || p.Category == "Wrap"
                                            || p.Category == "Burger");

                        if (pizzaCount == 0 && grillCount == 0)
                        {
                            // We might have an order with just drinks and dips
                            query.Parameters.AddWithValue("@status", "Order Ready");
                            query.Parameters.AddWithValue("@pizzaReady", true);
                            query.Parameters.AddWithValue("@grillReady", true);
                        } 
                        else if (pizzaCount == 0)
                        {
                            query.Parameters.AddWithValue("@status", "New");
                            query.Parameters.AddWithValue("@pizzaReady", true);
                            query.Parameters.AddWithValue("@grillReady", false);
                        }
                        else if (grillCount == 0)
                        {
                            query.Parameters.AddWithValue("@status", "New");
                            query.Parameters.AddWithValue("@pizzaReady", false);
                            query.Parameters.AddWithValue("@grillReady", true);
                        }
                        else
                        {
                            query.Parameters.AddWithValue("@status", "New");
                            query.Parameters.AddWithValue("@pizzaReady", false);
                            query.Parameters.AddWithValue("@grillReady", false);
                        }

                        // Here we check if we need to update the collection_time column
                        if (order.OrderType == "Collection")
                        {
                            query.Parameters.AddWithValue("@collectionTime", order.CollectionTime);
                        }
                        else
                        {
                            query.Parameters.AddWithValue("@collectionTime", DBNull.Value);
                        }

                        // Here we handle the case of cash on delivery
                        if (order.OrderType == "Delivery" && order.Payments["Cash"].Count > 0)
                        {
                            query.Parameters.AddWithValue("@orderPaid", false);
                        }
                        else if (order.OrderType == "Collection" && order.OrderSource == "Phone" && order.Payments["Cash"].Count > 0)
                        {
                            query.Parameters.AddWithValue("@orderPaid", false);
                        }
                        else
                        {
                            query.Parameters.AddWithValue("@orderPaid", true);
                        }

                        // Check if the order has a promotion
                        if (order.Promo != null)
                        {
                            query.Parameters.AddWithValue("@promoId", order.Promo.ID);
                        } 
                        else
                        {
                            query.Parameters.AddWithValue("@promoId", DBNull.Value);
                        }

                        if (query.ExecuteNonQuery() > 0)
                        {
                            EventLogger.LogInfo("Successfully completed checkout process");
                            return true;
                        }
                        return false;
                    }
                });
            }
            catch (Exception ex)
            {
                EventLogger.LogError("Error occured updating order: " + ex.Message);
            }
            return false;
        }

        // We create a stored procedure in MSSQL to handle order creation
        // this allows us to create tables containing orders / toppings
        // and insert orders in a single DB transaction
        private static (DataTable products, DataTable toppings) CreateTVPs(List<Product> products)
        {
            DataTable productTable = new DataTable();
            productTable.Columns.Add("product_id", typeof(int));
            productTable.Columns.Add("second_half_id", typeof(int));
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
                        product.SecondHalfID ?? (object)DBNull.Value,
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

        public int CreateOrder(Order order)
        {
            var (productTable, toppingTable) = CreateTVPs(order.OrderProducts.ToList());

            string storedProcedure = "CreateOrder";

            try
            {
                return _databaseManager.ExecuteQuery(conn =>
                {
                    using (SqlCommand query = new SqlCommand(storedProcedure, conn))
                    {
                        query.CommandType = CommandType.StoredProcedure;

                        query.Parameters.AddWithValue("@userID", order.UserID);
                        query.Parameters.AddWithValue("@orderType", order.OrderType);
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

        public Order? GetOrderByOrderNumber(int orderNumber)
        {
            string queryString = @"
                SELECT 
	                o.order_id, 
                    u.user_id,
	                u.name, 
	                o.order_date, 
                    o.order_type,
                    o.delivery_fee,
                    os.status_name
                FROM Orders AS o
                INNER JOIN Users AS u
	                ON o.user_id = u.user_id
                INNER JOIN Order_Status AS os
	                ON o.order_status_id = os.order_status_id
                WHERE order_id = @orderNumber;";

            try
            {
                Order? currentOrder = _databaseManager.ExecuteQuery(conn => {
                    using (SqlCommand query = new SqlCommand(queryString, conn))
                    {
                        query.Parameters.AddWithValue("@orderNumber", orderNumber);

                        using (SqlDataReader reader = query.ExecuteReader())
                        {
                            if (reader.HasRows && reader.Read())
                            {
                                int orderId = reader.IsDBNull(reader.GetOrdinal("order_id")) ? 0 : Convert.ToInt32(reader["order_id"]);
                                int userID = reader.IsDBNull(reader.GetOrdinal("user_id")) ? 0 : Convert.ToInt32(reader["user_id"]);
                                string? userName = reader.IsDBNull(reader.GetOrdinal("name")) ? null : reader["name"].ToString();
                                DateTime? orderDate = reader.IsDBNull(reader.GetOrdinal("order_date")) ? null : Convert.ToDateTime(reader["order_date"]);
                                string? orderType = reader.IsDBNull(reader.GetOrdinal("order_type")) ? null : reader["order_type"].ToString();
                                decimal? deliveryFee = reader.IsDBNull(reader.GetOrdinal("delivery_fee")) ? null : Convert.ToDecimal(reader["delivery_fee"]);
                                string? orderStatus = reader.IsDBNull(reader.GetOrdinal("status_name")) ? null : reader["status_name"].ToString();

                                if (orderId != 0 && orderDate != null && orderStatus != null)
                                {
                                    return new Order
                                    {
                                        ID = orderId,
                                        UserID = userID,
                                        OrderDate = (DateTime)orderDate,
                                        OrderType = orderType,
                                        OrderStatus = orderStatus,
                                        DeliveryFee = deliveryFee
                                    };
                                }                                
                            }
                            return null;
                        }
                    }
                });

                if (currentOrder != null)
                {
                    GetOrderProducts(currentOrder.ID).ForEach(p =>
                    {
                        currentOrder.OrderProducts.Add(p);
                    });                    

                    return currentOrder;
                }
                return null;
            }
            catch (Exception ex)
            {
                EventLogger.LogError("Error retrieving order " + ex.Message);
            }
            return null;
        }

        // Helper function to retrieve products associated with an order
        private List<Product> GetOrderProducts(int orderId)
        {
            // To handle meal deals we switch out the product name / category when the query returns null values 
            // if the product is a half and half pizza we grab the name of the second half
            string queryString = @"
                SELECT 
	                p.product_id, 
	                op.order_product_id, 
	                CASE
                        WHEN op.second_half_id IS NULL
                            THEN ISNULL(p.product_name, md.deal_name)
                        ELSE 
                            CONCAT(
                                p.product_name,
                                ' / ',
                                (SELECT product_name FROM Products WHERE product_id = op.second_half_id)
                            )
                        END AS product_name,
                    (SELECT ISNULL(p.product_category,'Deal')) AS product_category,
	                s.size_name, 
	                (SELECT ISNULL(pp.price,md.price)) AS price,	
	                op.deal_id,
	                op.deal_instance_id,
					STRING_AGG((a.allergen_description), ',') as allergens
                FROM Order_Products as op
                LEFT JOIN Products AS p
	                ON op.product_id = p.product_id
                LEFT JOIN sizes AS s
	                ON op.size_id = s.size_id
                LEFT JOIN Product_Prices AS pp
	                ON pp.product_id = p.product_id AND pp.size_id = s.size_id
                Left JOIN Meal_Deals AS md
	                ON op.deal_id = md.deal_id
				LEFT JOIN Product_Allergens AS pa
					ON pa.product_id = p.product_id
				LEFT JOIN Allergens AS a
					ON a.allergen_id = pa.allergen_id
                WHERE op.order_id = @orderId
				GROUP BY op.order_product_id, op.deal_id, op.deal_instance_id, s.size_name, op.second_half_id, 
                p.product_id, product_name, product_category, deal_name, md.price, pp.price";

            try
            {
                List<Product> products = _databaseManager.ExecuteQuery(conn =>
                {
                    using (SqlCommand query = new SqlCommand(queryString, conn))
                    {
                        query.Parameters.AddWithValue("@orderId", orderId);

                        using (SqlDataReader reader = query.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                List<Product> products = [];

                                while (reader.Read())
                                {                                    
                                    int? productId = reader.IsDBNull(reader.GetOrdinal("product_id")) ? null : Convert.ToInt32(reader["product_id"]);
                                    int orderProductId = reader.IsDBNull(reader.GetOrdinal("order_product_id")) ? 0 : Convert.ToInt32(reader["order_product_id"]);
                                    string? productName = reader.IsDBNull(reader.GetOrdinal("product_name")) ? string.Empty : reader["product_name"].ToString();
                                    string? productCategory = reader.IsDBNull(reader.GetOrdinal("product_category")) ? string.Empty : reader["product_category"].ToString();
                                    string? sizeName = reader.IsDBNull(reader.GetOrdinal("size_name")) ? string.Empty : reader["size_name"].ToString();
                                    decimal price = reader.IsDBNull(reader.GetOrdinal("price")) ? 0 : Convert.ToDecimal(reader["price"]);
                                    int? dealId = reader.IsDBNull(reader.GetOrdinal("deal_id")) ? null : Convert.ToInt32(reader["deal_id"]);
                                    int? dealInstanceId = reader.IsDBNull(reader.GetOrdinal("deal_instance_id")) ? null : Convert.ToInt32(reader["deal_instance_id"]);
                                    string? allergens = reader.IsDBNull(reader.GetOrdinal("allergens")) ? null : reader["allergens"].ToString();

                                    // Check if the product is a deal item
                                    if (productCategory != null && productCategory == "Deal" && dealId != null && dealInstanceId != null)
                                    {
                                        products.Add(new Product
                                        {
                                            ID = (int)dealId,
                                            ParentDealID = (int)dealInstanceId,
                                            Category = productCategory,
                                            Name = productName ?? "",
                                            Price = price,
                                            OrderProductId = orderProductId
                                        });                                        
                                    } // Check if the product is a member of a deal - if so set the price to 0 
                                    else if (dealId == null && productId != null && dealInstanceId != null)
                                    {
                                        products.Add(new Product { 
                                            ID = (int)productId,
                                            ParentDealID = dealInstanceId,
                                            Category = productCategory ?? "",
                                            Name = productName ?? "",
                                            SizeName = sizeName,
                                            Price = 0,
                                            Allergens = allergens == null ? [] : [..allergens.Split(',')],
                                            OrderProductId = orderProductId
                                        });
                                    } // Anything else is a normal product - still check for nulls
                                    else if (productId != null && productName != null && productCategory != null)
                                    {
                                        products.Add(new Product
                                        {
                                            ID = (int)productId,
                                            Category = productCategory,
                                            Name = productName,
                                            SizeName = sizeName,
                                            Price = price,
                                            Allergens = allergens == null ? [] : [..allergens.Split(',')], 
                                            OrderProductId = orderProductId
                                        });
                                    }
                                }
                                return products;
                            }
                            return [];
                        }
                    }
                });


                // Get toppings
                products.ForEach(p =>
                {
                    // We only need to get toppings for categories that allow them
                    if (p.Category == "Pizza" || p.Category == "Kebab")
                    {                        
                        GetProductToppings(p.OrderProductId).ForEach(t => {
                            // If base or bread add to required choices
                            if (t.ChoiceRequired)
                            {
                                p.RequiredChoices.Add(t);
                            }
                            else
                            {
                                p.Toppings.Add(t);
                            }
                        });
                    }                    
                });
                return products;
            }
            catch (Exception ex) 
            {
                EventLogger.LogError("Failed to fetch order products " + ex.Message);
            }
            return [];
        }

        // Helper function to retrieve product toppings
        private List<Topping> GetProductToppings(int orderProductId)
        {
            string queryString = @"
                SELECT 
                    t.topping_id, 
                    t.topping_name, 
                    tp.price,
                     CASE
	                    WHEN tt.topping_type IN ('Base', 'Bread')
                        THEN CAST(1 AS bit)
                        ELSE CAST(0 AS bit)
                     END AS choice_required,
					STRING_AGG((a.allergen_description), ',') as allergens
                    FROM Order_Product_Toppings AS opt
                    LEFT JOIN Order_Products AS op
	                    ON op.order_product_id = opt.order_product_id
                    LEFT JOIN Toppings AS t
	                    ON t.topping_id = opt.topping_id
					LEFT JOIN Topping_Allergens AS ta
						ON ta.topping_id = t.topping_id
					LEFT JOIN Allergens AS a
						ON a.allergen_id = ta.allergen_id
                    LEFT JOIN Topping_Types AS tt
	                    ON tt.topping_type_id = t.topping_type_id
                    LEFT JOIN Sizes AS s
	                    ON op.size_id = s.size_id
                    LEFT JOIN Topping_Prices AS tp
	                    ON op.size_id = tp.size_id AND tt.topping_type_id = tp.topping_type_id
                    WHERE op.order_product_id = @orderProductId
					GROUP BY tt.topping_type, t.topping_name, t.topping_id, tp.price;";

            try
            {
                List<Topping> toppings = _databaseManager.ExecuteQuery(conn =>
                {
                    using (SqlCommand query = new SqlCommand(queryString, conn))
                    {
                        query.Parameters.AddWithValue("@orderProductId", orderProductId);

                        using (SqlDataReader reader = query.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                List<Topping> toppings = [];

                                while (reader.Read())
                                {
                                    int toppingId = reader.IsDBNull(reader.GetOrdinal("topping_id")) ? 0 : Convert.ToInt32(reader["topping_id"]);
                                    string? toppingName = reader.IsDBNull(reader.GetOrdinal("topping_name")) ? string.Empty : reader["topping_name"].ToString();
                                    decimal price = reader.IsDBNull(reader.GetOrdinal("price")) ? 0 : Convert.ToDecimal(reader["price"]);
                                    bool choiceRequired = reader.IsDBNull(reader.GetOrdinal("choice_required")) ? false : Convert.ToBoolean(reader["choice_required"]);
                                    string? allergens = reader.IsDBNull(reader.GetOrdinal("allergens")) ? null : reader["allergens"].ToString();

                                    if (toppingName != null)
                                    {
                                        toppings.Add(new Topping
                                        {
                                            ID = toppingId,
                                            Name = toppingName,
                                            Price = price,
                                            ChoiceRequired = choiceRequired,
                                            Allergens = allergens == null ? [] : [..allergens.Split(',')]
                                        });
                                    }
                                }
                                return toppings;
                            }
                            return [];
                        }
                    }
                });
                return toppings;
            }            
            catch (Exception ex)
            {
                EventLogger.LogError("Failed to fetch order product toppings " + ex.Message);
            }
            return [];
        }

        public ObservableCollection<Order> GetKitchenOrders(bool pizzas)
        {
            // We get all orders for each station
            string queryString = @$"
                SELECT
                    o.order_id,
                    o.paid,
                    o.grill_ready,
                    o.pizza_ready,
                    os.status_name
                FROM Orders AS o
                INNER JOIN Order_Status AS os
                ON o.order_status_id = os.order_status_id
                WHERE os.status_name IN ('New', 'Preparing')
                AND {(pizzas ? "o.pizza_ready" : "o.grill_ready")} = 0;";

            try
            {
                ObservableCollection<Order> orders = _databaseManager.ExecuteQuery(conn =>
                {
                    using (SqlCommand query = new SqlCommand(queryString, conn))
                    {
                        using (SqlDataReader reader = query.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                ObservableCollection<Order> orders = []; 
                                while (reader.Read())
                                {
                                    int orderId = reader.IsDBNull(reader.GetOrdinal("order_id")) ? 0 : Convert.ToInt32(reader["order_id"]);
                                    bool? paid = reader.IsDBNull(reader.GetOrdinal("paid")) ? null : Convert.ToBoolean(reader["paid"]);
                                    bool? grillReady = reader.IsDBNull(reader.GetOrdinal("grill_ready")) ? null : Convert.ToBoolean(reader["grill_ready"]);
                                    bool? pizzaReady = reader.IsDBNull(reader.GetOrdinal("pizza_ready")) ? null : Convert.ToBoolean(reader["pizza_ready"]);
                                    string? statusName = reader.IsDBNull(reader.GetOrdinal("status_name")) ? null : reader["status_name"].ToString();

                                    if (
                                        orderId != 0
                                        && paid != null 
                                        && statusName != null
                                        && grillReady != null
                                        && pizzaReady != null
                                    )
                                    {
                                        orders.Add(new Order
                                        {
                                            ID = orderId,
                                            Paid = (bool)paid,
                                            GrillReady = (bool)grillReady,
                                            PizzaReady = (bool)pizzaReady,
                                            OrderStatus = statusName
                                        });
                                    }
                                }
                                return orders;
                            }
                            return [];
                        }
                    }
                });

                // Here we filter the products to retrieve for each order depending on the station
                foreach (Order o in orders)
                {
                    if (pizzas)
                    {
                        GetOrderProducts(o.ID).ForEach(p => {
                            if (p.Category == "Pizza")
                                o.OrderProducts.Add(p);
                        });
                    } 
                    else
                    {
                        GetOrderProducts(o.ID).ForEach(p =>
                        {
                            if (p.Category != "Pizza" && p.Category != "Drink" && p.Category != "Dip")
                                o.OrderProducts.Add(p);
                        });
                    }
                        
                }

                return orders;
            }
            catch (Exception ex)
            {
                EventLogger.LogError("Failed to fetch kitchen orders: " + ex.Message);
            }

            return [];
        }

        public ObservableCollection<Order> GetCollectionOrders()
        {
            // We get all orders and sort by status in the viewmodel
            string queryString = @"
                SELECT
                    o.order_id,
                    o.paid,
                    o.collection_time,
                    os.status_name
                FROM Orders AS o
                INNER JOIN Order_Status AS os
                ON o.order_status_id = os.order_status_id
                WHERE o.order_type = 'Collection' AND os.status_name != 'Completed'";
            
            try
            {
                ObservableCollection<Order> orders = _databaseManager.ExecuteQuery(conn =>
                {
                    using (SqlCommand query = new SqlCommand(queryString, conn))
                    {
                        using (SqlDataReader reader = query.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                ObservableCollection<Order> orders = [];
                                while (reader.Read())
                                {
                                    int orderId = reader.IsDBNull(reader.GetOrdinal("order_id")) ? 0 : Convert.ToInt32(reader["order_id"]);
                                    bool? paid = reader.IsDBNull(reader.GetOrdinal("paid")) ? null : Convert.ToBoolean(reader["paid"]);
                                    DateTime? collectionTime = reader.IsDBNull(reader.GetOrdinal("collection_time")) ? null : Convert.ToDateTime(reader["collection_time"]);
                                    string? statusName = reader.IsDBNull(reader.GetOrdinal("status_name")) ? null : reader["status_name"].ToString();

                                    if (
                                        orderId != 0
                                        && paid != null
                                        && statusName != null
                                    )
                                    {
                                        orders.Add(new Order { 
                                            ID = orderId,
                                            Paid = (bool)paid,
                                            CollectionTime = collectionTime,
                                            OrderStatus = statusName
                                        });
                                    }
                                }
                                return orders;
                            }
                            return [];
                        }
                    }
                });

                foreach (Order o in orders)
                {
                    GetOrderProducts(o.ID).ForEach(p =>
                    {
                        // We only need to add the items the cashier is responsible for
                        if (p.Category == "Dip" || p.Category == "Drink")
                            o.OrderProducts.Add(p);
                    });
                }

                return orders;
            }
            catch (Exception ex)
            {
                EventLogger.LogError("Failed to fetch collection orders: " + ex.Message);
            }
            return [];
        }

        public ObservableCollection<Order> GetDeliveryOrders()
        {
            string queryString = @"
                SELECT
                    o.order_id,
                    o.customer_id,
                    o.paid,
                    os.status_name
                FROM Orders AS o
                INNER JOIN Order_Status AS os
                ON o.order_status_id = os.order_status_id
                WHERE o.order_type = 'Delivery' AND os.status_name IN ('Order Ready','Out For Delivery')";

            try
            {
                // We will get the customer info in the view model
                return _databaseManager.ExecuteQuery(conn =>
                {
                    using (SqlCommand query = new SqlCommand(queryString, conn))
                    {
                        using (SqlDataReader reader = query.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                ObservableCollection<Order> orders = [];
                                while (reader.Read())
                                {
                                    int orderId = reader.IsDBNull(reader.GetOrdinal("order_id")) ? 0 : Convert.ToInt32(reader["order_id"]);
                                    int customerID = reader.IsDBNull(reader.GetOrdinal("customer_id")) ? 0 : Convert.ToInt32(reader["customer_id"]);
                                    bool? paid = reader.IsDBNull(reader.GetOrdinal("paid")) ? null : Convert.ToBoolean(reader["paid"]);
                                    string? statusName = reader.IsDBNull(reader.GetOrdinal("status_name")) ? null : reader["status_name"].ToString();

                                    if (
                                        orderId != 0
                                        && customerID != 0 
                                        && paid != null
                                        && statusName != null
                                    )
                                    {
                                        orders.Add(new Order { 
                                            ID = orderId,
                                            CustomerID = customerID,
                                            Paid = (bool)paid,
                                            OrderStatus = statusName
                                        });
                                    }
                                }
                                return orders;
                            }
                            return [];
                        }
                    }
                });                
            }
            catch (Exception ex)
            {
                EventLogger.LogError("Failed to fetch delivery orders: " + ex.Message);
            }
            return [];
        }

        // Update the database whenever a payment is made
        public bool CreatePayment(int orderId, decimal amount, string paymentType)
        {
            string queryString = @"
                DECLARE @payment_id INT;

                INSERT INTO Payments
                VALUES (@paymentType, @amount);

                SELECT @payment_id = SCOPE_IDENTITY();

                INSERT INTO Order_Payments
                VALUES (@payment_id, @orderId);";

            try
            {
                return _databaseManager.ExecuteQuery(conn =>
                {
                    using (SqlCommand query = new SqlCommand(queryString, conn))
                    {
                        query.Parameters.AddWithValue("@paymentType", paymentType);
                        query.Parameters.AddWithValue("@amount", amount);
                        query.Parameters.AddWithValue("@orderId", orderId);

                        if (query.ExecuteNonQuery() > 0)
                        {
                            EventLogger.LogInfo($"Payment for order {orderId} successful.");
                            return true;
                        }
                        EventLogger.LogError($"Unable to process payment for order {orderId}");
                        return false;
                    }
                });
            }
            catch (Exception ex)
            {
                EventLogger.LogError("Error occured during payment: " + ex.Message);
            }

            return false;
        }

        // We could write a procedure to update the existing order when changes are made before payment
        // but for the prototype it is simpler to just delete the initial order and create a new one,
        // we can also handle a change in order type this way
        public bool DeleteOrder(int orderId)
        {
            string storedProcedure = "DeleteOrder";

            try
            {
                return _databaseManager.ExecuteQuery(conn =>
                {
                    using (SqlCommand query = new SqlCommand(storedProcedure, conn))
                    {
                        query.CommandType = CommandType.StoredProcedure;

                        query.Parameters.AddWithValue("@orderID", orderId);                        

                        // If this operation fails it should throw an exception
                        query.ExecuteNonQuery();
                                                
                        return true;                        
                    }
                });
            }
            catch (Exception ex)
            {
                EventLogger.LogError("Failed to delete order: " + ex.Message);
            }
            return false;
        }

        // We can't return multiple values with ExecuteQuery so we create a datatype to return
        private struct OpeningTimes(TimeSpan open, TimeSpan close)
        {
            public TimeSpan Open = open;
            public TimeSpan Close = close;
            // Peak times don't change so we can hardcode these values
            public TimeSpan PeakStart = new(18, 00, 00);
            public TimeSpan PeakEnd = new(21, 00, 00);
        }        

        // Function to check if we can accept a collection order and a list of available collection time slots if we can
        public (bool, ObservableCollection<string>) GetCollectionTimes()
        {
            OpeningTimes times = GetOpeningTimes();

            TimeSpan now = DateTime.Now.TimeOfDay;                        

            // Collections will be returned in 15 minute intervals - 10:00, 10:15 etc.            
            TimeSpan collectionSlotInterval = new(00, 15, 00);
            // Expected time for order to be ready - longer during peak hours
            TimeSpan prepTime = now > times.PeakStart && now < times.PeakEnd ? new TimeSpan(00, 25, 00) : new TimeSpan(00, 15, 00);

            TimeSpan orderReady = now.Add(prepTime);

            // Only accept orders that will be ready 15 minutes before closing
            if (orderReady >= times.Close.Subtract(collectionSlotInterval))
            {
                return (false, ["Too late to order"]);
            } else if (orderReady <= times.Open.Add(collectionSlotInterval))
            {
                return (false, ["Too early to order"]);
            }            

            // check if the next slot is a 15 minute interval
            int remainder = orderReady.Minutes % collectionSlotInterval.Minutes;
            
            if (remainder != 0)
            {
                // if our current slot isn't the correct interval subtract the remainder
                orderReady = orderReady.Subtract(new TimeSpan(00, remainder, 00));
                // and add the interval again for the next available collection time
                orderReady = orderReady.Add(collectionSlotInterval);                
            }

            ObservableCollection<string> collectionSlots = [];
            
            while (orderReady < times.Close.Subtract(collectionSlotInterval))
            {
                // Adds all the available collection slots to the list
                collectionSlots.Add(orderReady.ToString(@"hh\:mm") + " PM");
                orderReady = orderReady.Add(collectionSlotInterval);
            }

            return (true, collectionSlots);
        }

        // Function to check if we will accept and order, and what time delivery can be expected
        public (bool, string) GetDeliveryTime()
        {
            OpeningTimes times = GetOpeningTimes();

            TimeSpan now = DateTime.Now.TimeOfDay;

            // Delivery time will be returned rounded up to the nearest 10 minute interval
            TimeSpan deliverySlotInterval = new TimeSpan(00, 10, 00);
            
            // 2 drivers available during peak times
            int availableDrivers = now > times.PeakStart && now < times.PeakEnd ? 2 : 1;
            
            // It takes 2-5 minutes to drive 1 mile in a city
            // the maximum time for a round trip in a 4-mile radius should be 40 minutes
            // If we have 2 drivers we half this value
            // This is just the initial logic we should really calculate this from the database
            TimeSpan expectedRoundTrip = availableDrivers > 1 ? new TimeSpan(00, 20, 00) : new TimeSpan(00, 40, 00);
            
            // Expected time for order to be ready - longer during peak hours
            TimeSpan prepTime = now > times.PeakStart && now < times.PeakEnd ? new TimeSpan(00, 25, 00) : new TimeSpan(00, 15, 00);

            TimeSpan orderReady = now.Add(prepTime);

            // If the shop closes before the time it takes to make / deliver the order we reject it
            if (orderReady > times.Close.Subtract(expectedRoundTrip))
            {
                return (false, "Too late\n to order");
            } // If the order is before the shop opens also reject it 
            else if (orderReady < times.Open.Add(prepTime))
            {
                // Changed while testing
                return (true, "Too early\n to order");
            }

            int remainder = orderReady.Minutes % deliverySlotInterval.Minutes;

            if (remainder != 0)
            {
                // if our current slot isn't the correct interval subtract the remainder
                orderReady = orderReady.Subtract(new TimeSpan(00, remainder, 00));
                // and add the interval again for the next available collection time
                orderReady = orderReady.Add(deliverySlotInterval);
            }

            return (true, orderReady.ToString(@"hh\:mm") + " PM");
        }


        // Helper function to get the opening times from the database
        private OpeningTimes GetOpeningTimes()
        {
            DayOfWeek currentDay = DateTime.Now.DayOfWeek;            

            string queryString = @"
                SELECT open_time, close_time 
                FROM Opening_Times
                WHERE day_name LIKE @currentDay;
            ";

            return _databaseManager.ExecuteQuery(conn =>
            {
                using (SqlCommand query = new SqlCommand(queryString, conn))
                {
                    query.Parameters.AddWithValue("@currentDay", currentDay.ToString());

                    using (SqlDataReader reader = query.ExecuteReader())
                    {
                        if (reader.HasRows && reader.Read())
                        {
                            int openTimeOrdinal = reader.GetOrdinal("open_time");
                            int closeTimeOrdinal = reader.GetOrdinal("close_time");

                            TimeSpan? open = reader.IsDBNull(openTimeOrdinal) ? null : reader.GetTimeSpan(openTimeOrdinal);
                            TimeSpan? close = reader.IsDBNull(closeTimeOrdinal) ? null : reader.GetTimeSpan(closeTimeOrdinal);

                            if (open != null && close != null)
                            {
                                return new OpeningTimes((TimeSpan)open, (TimeSpan)close);
                            }                            
                        }
                        return default!;
                    }
                }
            });            
        }

        public bool PrepareOrder(int orderNumber)
        {
            string queryString = @"
                UPDATE Orders
                SET order_status_id = (SELECT order_status_id FROM Order_Status WHERE status_name = 'Preparing')
                WHERE order_id = @orderNumber;";

            try
            {
                return _databaseManager.ExecuteQuery(conn =>
                {
                    using (SqlCommand query = new SqlCommand(queryString, conn))
                    {
                        query.Parameters.AddWithValue("@orderNumber", orderNumber);

                        if (query.ExecuteNonQuery() > 0)
                        {
                            EventLogger.LogInfo("Successfully updated order prep");
                            return true;
                        }
                        return false;
                    }
                });
            }
            catch (Exception ex)
            {
                EventLogger.LogError("Failed to update order prep: " + ex.Message);
            }
            return false;
        }

        public bool CompleteOrderStation(int orderNumber, bool pizza)
        {
            string queryString = $@"
                UPDATE Orders
                SET {(pizza ? "pizza_ready" : "grill_ready")} = 1
                WHERE order_id = @orderNumber";

            try
            {
                return _databaseManager.ExecuteQuery(conn =>
                {
                    using (SqlCommand query = new SqlCommand(queryString, conn))
                    {
                        query.Parameters.AddWithValue("@orderNumber", orderNumber);

                        if (query.ExecuteNonQuery() > 0)
                        {
                            EventLogger.LogInfo("Successfully updated station completion");
                            return true;
                        }
                        return false;
                    }
                });
            }
            catch (Exception ex)
            {
                EventLogger.LogError("Failed to update station completion " + ex.Message);
            }
            return false;
        }

        public bool OrderReady(int orderNumber)
        {
            string queryString = @"
                UPDATE Orders
                SET grill_ready = 1,
                pizza_ready = 1,
                order_status_id = (SELECT order_status_id FROM Order_Status WHERE status_name = 'Order Ready')
                WHERE order_id = @orderNumber;";

            try
            {
                return _databaseManager.ExecuteQuery(conn =>
                {
                    using (SqlCommand query = new SqlCommand(queryString, conn))
                    {
                        query.Parameters.AddWithValue("@orderNumber", orderNumber);

                        if (query.ExecuteNonQuery() > 0)
                        {
                            EventLogger.LogInfo("Successfully completed order");
                            return true;
                        }
                        return false;
                    }
                });
            }
            catch (Exception ex)
            {
                EventLogger.LogError("Failed to complete order " + ex.Message);
            }
            return false;
        }

        public bool CompleteOrder(int orderNumber)
        {
            string queryString = @"
                UPDATE Orders
                SET order_status_id = (SELECT order_status_id FROM Order_Status WHERE status_name = 'Completed')
                WHERE order_id = @orderNumber;";

            try
            {
                return _databaseManager.ExecuteQuery(conn =>
                {
                    using (SqlCommand query = new SqlCommand(queryString, conn))
                    {
                        query.Parameters.AddWithValue("@orderNumber", orderNumber);

                        if (query.ExecuteNonQuery() > 0)
                        {
                            EventLogger.LogInfo("Successfully completed order");
                            return true;
                        }
                        return false;
                    }
                });
            }
            catch (Exception ex)
            {
                EventLogger.LogError("Failed to complete order " + ex.Message);
            }
            return false;            
        }
    }
    
}
