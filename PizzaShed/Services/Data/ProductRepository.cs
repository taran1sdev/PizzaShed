using Microsoft.Data.SqlClient;
using PizzaShed.Model;
using PizzaShed.Services.Logging;
using System.Windows.Controls;

namespace PizzaShed.Services.Data
{
    internal class ProductRepository : IProductRepository
    {
        private readonly DatabaseManager _databaseManager = DatabaseManager.Instance;

        public List<Product> GetProductsByCategory(string category)
        {
            try
            {
                // Call the execute query function that handles the DB connection
                List<Product> products = _databaseManager.ExecuteQuery(conn =>
                {
                    string queryString = @"
                    SELECT 
                        p.product_id,
                        p.product_name,
                        p.product_category,
                        ps.size_name,
                        ps.price,
                        STRING_AGG((a.allergen_description), ',') as allergens
                        FROM Products AS p
                        LEFT JOIN Product_Sizes AS ps
                            ON p.product_id = ps.product_id
                        LEFT JOIN Product_Allergens as pa
                            ON p.product_id = pa.product_id
                        LEFT JOIN Allergens as a
                            ON pa.allergen_id = a.allergen_id
                        WHERE p.product_category = @category
                        GROUP BY p.product_id, p.product_name, p.product_category, ps.size_name, ps.price
                        ORDER BY p.product_name, ps.price;
                    ";

                    using (SqlCommand query = new(queryString, conn)) 
                    {
                        query.Parameters.AddWithValue("@category", category);

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
                                    string? name = reader["product_name"].ToString();
                                    string? category = reader["product_category"].ToString();
                                    string? sizeName = reader["size_name"].ToString();
                                    decimal? price = Convert.ToDecimal(reader["price"]);
                                    string? allergensStr = reader["allergens"].ToString();

                                    if (name != null 
                                    && category != null 
                                    && sizeName != null
                                    && price != null)
                                    {
                                        products.Add(new Product
                                        {
                                            ProductId = id,
                                            Name = name,
                                            Category = category,   
                                            SizeName = sizeName,
                                            Price = (decimal)price,
                                            Allergens = allergensStr == null ? [] : allergensStr.Split(',')
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
                EventLogger.LogError("MenuItemsRepository.GetProductsByCategory(category) - Failed to retrieve products from Database: " + ex.Message);                
            }
            return [];
        }

        public List<Product> GetProductsByCategory(string category, string size)
        {
            return default!;
        }

        public List<Product> GetToppingsByCategory(string category)
        {
            return default!;
        }
    }
}
