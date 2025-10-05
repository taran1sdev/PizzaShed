using Microsoft.Data.SqlClient;
using PizzaShed.Model;
using PizzaShed.Services.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaShed.Services.Data
{
    public class ToppingRepository : IProductRepository<Topping>
    {
        private readonly DatabaseManager _databaseManager = DatabaseManager.Instance;

        public List<Topping> GetProductsByCategory(string category, string? size)
        {
            try
            {
                // Call the execute query function that handles the DB connection
                List<Topping> toppings = _databaseManager.ExecuteQuery(conn =>
                {
                    // This query is quite long but it returns the toppings allowed for each product
                    // category and marks which ones require a user choice we use DISTINCT for the Kebab toppings                    
                    string queryString = $@"
                    SELECT {(category == "Kebab" ? "DISTINCT" : "")}
                        t.topping_id,
                        t.topping_name,
                        tp.price,
                        tt.topping_type,
                        CASE
                            WHEN tt.topping_type IN ('Base', 'Bread')
                            THEN CAST(1 AS bit)
                            ELSE CAST(0 AS bit)
                        END AS choice_required,
                        -- This will return duplicates but we handle in C#
                        STRING_AGG(a.allergen_description, ',') as allergens
                        FROM Toppings AS t
                        INNER JOIN Topping_Types as tt
                            ON t.topping_type_id = tt.topping_type_id
                        INNER JOIN Topping_Prices AS tp
                            ON tt.topping_type_id = tp.topping_type_id
                        INNER JOIN Allowed_Product_Categories AS apc
                            ON tt.topping_type_id = apc.topping_type_id
                        INNER JOIN Sizes AS s
                            ON tp.size_id = s.size_id                        
                        LEFT JOIN Topping_Allergens as ta
                            ON t.topping_id = ta.topping_id
                        LEFT JOIN Allergens as a
                            ON ta.allergen_id = a.allergen_id
                        LEFT JOIN Product_Toppings as pt
                            ON tt.topping_type_id = pt.topping_type_id                        
                        WHERE apc.product_category = @category
                        {(size != null && category != "Kebab" ? "AND s.size_name = @size" : "")}
                        GROUP BY tt.topping_type, t.topping_id, t.topping_name, s.size_name, tp.price";

                    using (SqlCommand query = new(queryString, conn))
                    {
                        query.Parameters.AddWithValue("@category", category);

                        // First check if we are querying for size
                        if (size != null)
                        {
                            query.Parameters.AddWithValue("@size", size);
                        }

                        using (SqlDataReader reader = query.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                // Only declare our return variable once we know we have data to return
                                List<Topping> toppings = [];

                                while (reader.Read())
                                {
                                    // Make sure none of required Properties contain null values before creating our object
                                    int id = reader.IsDBNull(reader.GetOrdinal("topping_id")) ? 0 : Convert.ToInt32(reader["topping_id"]); // Private key should never be null but we still check
                                    string? name = reader.IsDBNull(reader.GetOrdinal("topping_name")) ? null : reader["topping_name"].ToString();
                                    decimal? price = reader.IsDBNull(reader.GetOrdinal("price")) ? 0 : Convert.ToDecimal(reader["price"]);
                                    bool? choiceRequired = reader.IsDBNull(reader.GetOrdinal("choice_required")) ? null : Convert.ToBoolean(reader["choice_required"]);
                                    string? allergensStr = reader.IsDBNull(reader.GetOrdinal("allergens")) ? null : reader["allergens"].ToString();
                                    string? toppingType = reader.IsDBNull(reader.GetOrdinal("topping_type")) ? null : reader["topping_type"].ToString();

                                    if (id != 0 
                                    && name != null
                                    && category != null
                                    && choiceRequired != null
                                    && price != null
                                    && toppingType != null)
                                    {
                                        toppings.Add(new Topping
                                        {
                                            ID = id,
                                            Name = name,
                                            Price = (decimal)price,
                                            ChoiceRequired = (bool)choiceRequired,
                                            ToppingType = toppingType,
                                            // Query returns duplicate values but HashSet will only store distinct values
                                            Allergens = allergensStr == null ? [] : [.. allergensStr.Split(',')]
                                        });
                                    }

                                }
                                return toppings;
                            } else                           
                            return [];                                                          
                        }
                    }
                });

                if (category.Equals("Pizza", StringComparison.OrdinalIgnoreCase))
                {
                    toppings = [.. toppings.OrderBy(t => t.ToppingType switch
                    {
                        "Meat" => 1,
                        "Base" => 2,
                        "Veg" => 3,
                        _ => 99
                    })];
                }
                return toppings;
            }
            catch (Exception ex)
            {
                EventLogger.LogError("ToppingRepository.GetProductsByCategory(category) - Failed to retrieve products from Database: " + ex.Message);
            }
            return [];
        }

        public List<Topping> GetProductsByCategory(string category)
        {
            return GetProductsByCategory(category, null);
        }

        public List<Topping> GetMealDeals()
        {
            return default!; // Not implemented here
        }
    }
}
