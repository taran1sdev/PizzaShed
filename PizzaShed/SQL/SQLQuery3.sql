SELECT * FROM Orders;

--SELECT 
--	o.order_id, 
--	u.name, 
--	os.status_name, 
--	o.order_date, 
--	o.collection_time, 
--	o.order_source, 
--	o.order_notes, 
--	o.order_type, 
--	o.total_price	
--FROM Orders AS o
--INNER JOIN Users AS u
--	ON o.user_id = u.user_id
--INNER JOIN Order_Status AS os
--	ON o.order_status_id = os.order_status_id
--WHERE order_id = 1;

--SELECT 
--	p.product_id, 
--	op.order_product_id, 
--	(SELECT ISNULL(p.product_name, md.deal_name)) AS product_name, 
--	(SELECT ISNULL(p.product_category,'Deal')) AS product_category,
--	s.size_name, 
--	(SELECT ISNULL(pp.price,md.price)) AS price,	
--	op.deal_id,
--	op.deal_instance_id	
--FROM Order_Products as op
--LEFT JOIN Products AS p
--	ON op.product_id = p.product_id
--LEFT JOIN sizes AS s
--	ON op.size_id = s.size_id
--LEFT JOIN Product_Prices AS pp
--	ON pp.product_id = p.product_id AND pp.size_id = s.size_id
--Left JOIN Meal_Deals AS md
--	ON op.deal_id = md.deal_id
--WHERE op.order_id = 5;

--SELECT t.topping_id, t.topping_name, tp.price,
-- CASE
--	WHEN tt.topping_type IN ('Base', 'Bread')
--    THEN CAST(1 AS bit)
--    ELSE CAST(0 AS bit)
-- END AS choice_required
--FROM Order_Product_Toppings AS opt
--LEFT JOIN Order_Products AS op
--	ON op.order_product_id = opt.order_product_id
--LEFT JOIN Toppings AS t
--	ON t.topping_id = opt.topping_id
--LEFT JOIN Topping_Types AS tt
--	ON tt.topping_type_id = t.topping_type_id
--LEFT JOIN Sizes AS s
--	ON op.size_id = s.size_id
--LEFT JOIN Topping_Prices AS tp
--	ON op.size_id = tp.size_id AND tt.topping_type_id = tp.topping_type_id
--WHERE op.order_product_id = 2;




SELECT 
	promo_id, 
	description,
	promo_code,
	discount_value,
	min_spend
FROM Promotions 
WHERE min_spend <= 15.0;
	