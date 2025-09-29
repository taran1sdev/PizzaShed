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
                        WHERE p.product_category ='pizza'
                        
                        GROUP BY  s.size_name, p.product_name, p.product_category, p.product_id, pp.price;
                        