using Microsoft.Data.SqlClient;
using PizzaShed.Model;
using PizzaShed.Services.Logging;
using System.Windows.Controls;

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
                                            ID = id,
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
                EventLogger.LogError("ProductRepository.GetProductsByCategory(category) - Failed to retrieve products from Database: " + ex.Message);                
            }
            return [];
        }

        public List<Product> GetProductsByCategory(string category)
        {
            return GetProductsByCategory(category, null);
        }
    }
}
