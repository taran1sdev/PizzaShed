using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using PizzaShed.Model;
using PizzaShed.Services.Logging;
using System.Reflection.Metadata;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace PizzaShed.Services.Data
{
    internal class ProductRepository : IProductRepository<Product>
    {
        private readonly DatabaseManager _databaseManager = DatabaseManager.Instance;

        public List<Product> GetProductsByCategory(string category, string? size)
        {
            try
            {
                // Call the execute query function that handles the DB connection
                List<Product> products = _databaseManager.ExecuteQuery(conn =>
                {
                    string queryString = $@"
                    SELECT
                        p.product_id,
                        p.product_name,
                        p.product_category,
                        s.size_name,
                        pp.price,
                        STRING_AGG((a.allergen_description), ',') as allergens
                        FROM Products AS p
                        LEFT JOIN Product_Prices AS pp
                            ON p.product_id = pp.product_id
                        LEFT JOIN Sizes AS s
                            ON pp.size_id = s.size_id
                        LEFT JOIN Product_Allergens as pa
                            ON p.product_id = pa.product_id
                        LEFT JOIN Allergens as a
                            ON pa.allergen_id = a.allergen_id
                        WHERE p.product_category = @category
                        {(size != null ? "AND s.size_name = @size" : "")}
                        GROUP BY p.product_id, s.size_name, p.product_name, p.product_category, pp.price
                        ORDER BY s.size_name DESC";

                    using (SqlCommand query = new(queryString, conn)) 
                    {
                        query.Parameters.AddWithValue("@category", category);
                        
                        // First check if we are querying for size
                        if(size != null)
                        {
                            query.Parameters.AddWithValue("@size", size);
                        }

                        using (SqlDataReader reader = query.ExecuteReader())
                        {                            
                            if (reader.HasRows)
                            {
                                // Only declare our return variable once we know we have data to return
                                List<Product> products = [];

                                while (reader.Read())
                                {
                                    // Make sure none of required Properties contain null values before creating our object
                                    int id = Convert.ToInt32(reader["product_id"]);
                                    string? name = reader.IsDBNull(reader.GetOrdinal("product_name")) ? null : reader["product_name"].ToString();
                                    string? category = reader.IsDBNull(reader.GetOrdinal("product_category")) ? null : reader["product_category"].ToString();
                                    string? sizeName = reader.IsDBNull(reader.GetOrdinal("size_name")) ? null : reader["size_name"].ToString();
                                    decimal? price = reader.IsDBNull(reader.GetOrdinal("price")) ? null : Convert.ToDecimal(reader["price"]);
                                    string? allergensStr = reader.IsDBNull(reader.GetOrdinal("allergens")) ? null : reader["allergens"].ToString();

                                    if (name != null 
                                    && category != null 
                                    && sizeName != null
                                    && price != null)
                                    {
                                        products.Add(new Product
                                        {                                            
                                            Name = name,
                                            Category = category,   
                                            SizeName = sizeName,
                                            Price = price,
                                            Allergens = allergensStr == null ? [] : [.. allergensStr.Split(',')]
                                        });
                                    }
                                        
                                }
                                return products;
                            }
                            return [];
                        }
                    }
                });
                return products;
            } catch (Exception ex)
            {
                EventLogger.LogError("Failed to fetch products: " + ex.Message);                
            }
            return [];
        }

        public List<Product> GetProductsByCategory(string category)
        {
            return GetProductsByCategory(category, null);
        }

        private Product GetProductById(int productId, int sizeId)
        {
            try
            {
                Product product = _databaseManager.ExecuteQuery(conn =>
                {
                    string queryString = @"
                        SELECT p.product_name, p.product_category, pp.price, s.size_name,
                        STRING_AGG((a.allergen_description), ',') as allergens
                        FROM Products AS p
                        INNER JOIN Product_Prices AS pp
                        ON p.product_id = pp.product_id
                        INNER JOIN Sizes AS s
                        ON s.size_id = pp.size_id
                        LEFT JOIN Product_Allergens AS pa
                        ON p.product_id = pa.product_id
                        LEFT JOIN Allergens AS a
                        ON pa.allergen_id = a.allergen_id
                        WHERE p.product_id = @product AND s.size_id = @size
                        GROUP BY p.product_category, p.product_name, pp.price, s.size_name
                        ";

                    using (SqlCommand query = new(queryString, conn))
                    {
                        query.Parameters.AddWithValue("@product", productId.ToString());
                        query.Parameters.AddWithValue("@size", sizeId.ToString());

                        using (SqlDataReader reader = query.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // If the Database contains a null value convert and ToString functions will throw an exception - this allows us to safely handle null values
                                string? name = reader.IsDBNull(reader.GetOrdinal("product_name")) ? null : reader["product_name"].ToString();
                                string? category = reader.IsDBNull(reader.GetOrdinal("product_category")) ? null : reader["product_category"].ToString();                                
                                string? sizeName = reader.IsDBNull(reader.GetOrdinal("size_name")) ? null : reader["size_name"].ToString();
                                string? allergensStr = reader.IsDBNull(reader.GetOrdinal("allergens")) ? null : reader["allergens"].ToString();             
                                
                                if (name != null && category != null && sizeName != null)
                                {
                                    return new Product
                                    {
                                        ID = productId,
                                        Name = name,
                                        Category = category,
                                        Price = 0,
                                        SizeName = sizeName,
                                        Allergens = allergensStr == null ? [] : [..allergensStr.Split(',')]
                                    };
                                }
                            }
                            return default!;
                        }
                    }
                });                
                return product;
            } catch (Exception ex)
            {
                EventLogger.LogError("Failed to fetch product: " + ex.Message);
            }
            return default!;
        }

        // Function for retrieving Meal Deals
        public List<Product> GetMealDeals()
        {
            try
            {
                List<Product> mealDeals = _databaseManager.ExecuteQuery(conn =>
                {
                    string queryString = @"SELECT deal_id, deal_name, price FROM Meal_Deals;";

                    using (SqlCommand query = new(queryString, conn))
                    {
                        using (SqlDataReader reader = query.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                List<Product> deals = [];

                                while (reader.Read())
                                {
                                    int dealId = reader.IsDBNull(reader.GetOrdinal("deal_id")) ? 0 : Convert.ToInt32(reader["deal_id"]);
                                        
                                    string? name = reader.IsDBNull(reader.GetOrdinal("deal_name")) ? null : reader["deal_name"].ToString();

                                    decimal? price = reader.IsDBNull(reader.GetOrdinal("price")) ? null : Convert.ToDecimal(reader["price"]);                                    

                                    if (name != null && price != null)
                                    {
                                        deals.Add(new Product
                                        {
                                            ID = dealId,
                                            Name = name,
                                            Category = "Deal",
                                            Price = price,                                                                                        
                                        });
                                    } else
                                    {
                                        EventLogger.LogError($"NULL values returned fetching meal deals Name:{name}, Price:{price}");
                                    }
                                }
                                return deals;
                            }
                            return [];                                                        
                        }
                    }                    
                });

                // We get the products in each deal before returning
                if (mealDeals.Count > 0 )
                {
                    mealDeals.ForEach(m =>
                    {
                        m.RequiredChoices = [.. GetDealItems(m.ID).Cast<MenuItemBase>()];
                    });
                }
                return mealDeals;
            } catch (Exception ex)
            {
                EventLogger.LogError("Failed to fetch meal deals: " + ex.Message);
            }
            return [];
        }

        // We create a datatype to store products we need to add later
        private struct ProductIds(int productId, int sizeId)
        {
            public int ProductId = productId;
            public int SizeId = sizeId;
        }         

        // This function returns the products in each deal
        private List<Product> GetDealItems(int dealId)
        {
            // We create a list of our new datatype to store the products we need to retrieve
            // once the DB connection is closed
            List<ProductIds> productsToAdd = [];
            
            try
            {                
                List<Product> dealItems = _databaseManager.ExecuteQuery(conn =>
                {
                    string queryString = @"
                        SELECT di.product_id, di.product_category, di.quantity, s.size_name, di.size_id
                        FROM Deal_Items AS di
                        INNER JOIN Sizes AS s
                        ON s.size_id = di.size_id
                        WHERE di.deal_id = @deal_id";

                    using (SqlCommand query = new(queryString, conn))
                    {
                        query.Parameters.AddWithValue("@deal_id", dealId);

                        using (SqlDataReader reader = query.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {                                
                                List<Product> dealItems = [];

                                while (reader.Read())
                                {                                    
                                    int product_id = reader.IsDBNull(reader.GetOrdinal("product_id")) ? 0 : Convert.ToInt32(reader["product_id"]);
                                    
                                    int quantity = reader.IsDBNull(reader.GetOrdinal("quantity")) ? 1 : Convert.ToInt32(reader["quantity"]);
                                    
                                    string? category = reader.IsDBNull(reader.GetOrdinal("product_category")) ? null : reader["product_category"].ToString();
                                    string? size_name = reader.IsDBNull(reader.GetOrdinal("size_id")) ? null : reader["size_name"].ToString();

                                    int size_id = reader.IsDBNull(reader.GetOrdinal("size_id")) ? 0 : Convert.ToInt32(reader["size_id"]);

                                    // Add an object for every deal item required
                                    for (int i = 0; i < quantity; i++)
                                    {
                                        if (product_id != 0 && size_id != 0)
                                        {
                                            // If the deal item cannot be chosen - add the product / size id to an object
                                            // We will retrieve the product once this DB connection is closed
                                            productsToAdd.Add(new ProductIds(product_id, size_id));
                                            continue;
                                        } 
                                        else
                                        {
                                            // If the deal item requires a choice - just add the category and size
                                            if (category != null && size_name != null)
                                            {
                                                dealItems.Add(new Product
                                                {        
                                                    ID = product_id,
                                                    Name = $"{category}",
                                                    Category = category,
                                                    Price = 0,
                                                    SizeName = size_name,
                                                });
                                            }                                            
                                        }
                                    }                                    
                                }                                                                
                                return dealItems;
                            }
                            return [];
                        }
                    }
                });

                // Now the connection is closed we can fetch the products
                productsToAdd.ForEach(p =>
                {
                    Product itemToAdd = GetProductById(p.ProductId, p.SizeId);                    
                    dealItems.Add(itemToAdd);

                    EventLogger.LogInfo($"{itemToAdd.Name} {itemToAdd.ID}");
                });

                return dealItems;
            }
            catch (Exception ex)
            {
                EventLogger.LogError("Failed to fetch deal items: " + ex.Message);
            }
            return [];
        }
    }
}
