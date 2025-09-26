INSERT INTO Users (name, PIN, role)
VALUES 
('Brent', 'ixNq2nSsWiZNu8fnqXAjVvIxHKLh56Nz5EHnszlYWZ0NE1M6WPMOMTP9tJ6PMruW', 'Manager'),
('Alice', 'xe2Awqv+pgXD3EX+EBSZ0O/5TUGTUjEacGhorHzsSyoCm+SM9jDdVa8oFvByDRdm', 'Cashier'),
('Luigi', '2Vkn7YlETfqU/FglS7qxFpg+oUOyE4p6dVu9g9tAA2LvjI0+VBF9mXyhz7L7Par9', 'Pizzaiolo'),
('Bob', 'jFtxP05ZZK75HmdFkeQscalH+vI874VgAsvV9lnRYQmwyXojjmYopN0oFGQ5HUd5', 'Grill Cook'),
('Bill', 'SwZWt1DtzjVf8SNkxclr61vaONDHwJfzgWyDyDyQYHfWvtCQ6U2cTZdXoP8FjSHr', 'Driver'),
('John', '0BZwpXpfgVstqyq7RGNexS8dZIw1sE0CDvH8e5SPN9O/Lblir6l7YSsNVg9t2tGf', 'Driver');

INSERT INTO Customers (name, phone_no, post_code, house_no, street_address)
VALUES ('Ali Wong', '01234 555111', 'TA6 4AB', 10, 'Oak Road'),
('Samir Patel', '01234 555222', 'TA6 5CD', 22, 'Bridge Street'),
('Jess Brown', '01234 555333', 'TA6 1EF', 7, 'Mill Close');

INSERT INTO Promotions (promo_code, description, discount_value, min_spend)
VALUES ('PIZZA10', '10% off when you spend £15', 0.1, 15.0),
(NULL, 'Student Discount', 0.1, 12.0);

INSERT INTO Products (product_name, product_category)
VALUES ('Margherita', 'Pizza'),
('Pepperoni', 'Pizza'),
('Hawaiaan', 'Pizza'),
('Veggie Supreme', 'Pizza'),
('BBQ Chicken', 'Pizza'),
('Meat Feast', 'Pizza'),
('Doner Delight', 'Pizza'), 
('Build-Your-Own', 'Pizza'),
('Doner Kebab', 'Kebab'),
('Chicken Shish', 'Kebab'),
('Mixed Kebab', 'Kebab'),
('Kebab Box', 'Kebab'),
('Cheeseburger', 'Burger'),
('Chicken Fillet Burger', 'Burger'),
('Doner Wrap', 'Wrap'),
('Chicken Wrap', 'Wrap'),
('Chips', 'Side'),
('Cheesy Chips', 'Side'),
('Garlic Bread', 'Side'),
('Chicken Wings', 'Side'),
('Onion Rings', 'Side'),
('Garlic Mayo', 'Dip'),
('Chilli', 'Dip'),
('Ketchup', 'Dip'),
('BBQ', 'Dip'),
('Burger Sauce', 'Dip'),
('Coke', 'Drink'),
('Diet Coke', 'Drink'),
('7UP', 'Drink'),
('Fanta', 'Drink');

INSERT INTO Product_Sizes
SELECT product_id, 'Small', 6.99
FROM Products 
WHERE product_name = 'Margherita';

INSERT INTO Product_Sizes
SELECT product_id, 'Medium', 9.99
FROM Products 
WHERE product_name = 'Margherita';

INSERT INTO Product_Sizes
SELECT product_id, 'Large', 11.49
FROM Products 
WHERE product_name = 'Margherita';


INSERT INTO Product_Sizes
SELECT product_id, 'Small', 7.99
FROM Products 
WHERE product_name IN ('Pepperoni', 'Hawaiian', 'Veggie Supreme');

INSERT INTO Product_Sizes
SELECT product_id, 'Medium', 9.99
FROM Products 
WHERE product_name IN ('Pepperoni', 'Hawaiian', 'Veggie Supreme');

INSERT INTO Product_Sizes
SELECT product_id, 'Large', 12.99
FROM Products 
WHERE product_name IN ('Pepperoni', 'Hawaiian', 'Veggie Supreme');

INSERT INTO Product_Sizes
SELECT product_id, 'Small', 8.49
FROM Products 
WHERE product_name = 'BBQ Chicken';

INSERT INTO Product_Sizes
SELECT product_id, 'Medium', 10.49
FROM Products 
WHERE product_name = 'BBQ Chicken';

INSERT INTO Product_Sizes
SELECT product_id, 'Large', 13.49
FROM Products 
WHERE product_name = 'BBQ Chicken';

INSERT INTO Product_Sizes
SELECT product_id, 'Small', 8.99
FROM Products 
WHERE product_name IN ('Meat Feast', 'Doner Delight');

INSERT INTO Product_Sizes
SELECT product_id, 'Medium', 10.99
FROM Products 
WHERE product_name IN ('Meat Feast', 'Doner Delight');

INSERT INTO Product_Sizes
SELECT product_id, 'Large', 13.99
FROM Products 
WHERE product_name IN ('Meat Feast', 'Doner Delight');

INSERT INTO Product_Sizes
SELECT product_id, 'Small', 6.49
FROM Products 
WHERE product_name = 'Build-Your-Own';

INSERT INTO Product_Sizes
SELECT product_id, 'Medium', 8.49
FROM Products 
WHERE product_name = 'Build-Your-Own';

INSERT INTO Product_Sizes
SELECT product_id, 'Large', 10.99
FROM Products 
WHERE product_name = 'Build-Your-Own';

INSERT INTO Product_Sizes
SELECT product_id, 'Regular', 6.49
FROM Products 
WHERE product_name = 'Doner Kebab';

INSERT INTO Product_Sizes
SELECT product_id, 'Large', 7.99
FROM Products 
WHERE product_name = 'Doner Kebab';

INSERT INTO Product_Sizes
SELECT product_id, 'Regular', 7.49
FROM Products 
WHERE product_name = 'Chicken Shish';

INSERT INTO Product_Sizes
SELECT product_id, 'Large', 8.99
FROM Products 
WHERE product_name = 'Chicken Shish';

INSERT INTO Product_Sizes
SELECT product_id, 'Regular', 9.49
FROM Products 
WHERE product_name = 'Mixed Kebab';

INSERT INTO Product_Sizes
SELECT product_id, 'Regular', 7.99
FROM Products 
WHERE product_name = 'Kebab Box';

INSERT INTO Product_Sizes
SELECT product_id, 'Regular', 5.49
FROM Products 
WHERE product_name = 'Cheesburger';

INSERT INTO Product_Sizes
SELECT product_id, 'Regular', 5.99
FROM Products 
WHERE product_name = 'Chicken Fillet Burger';

INSERT INTO Product_Sizes
SELECT product_id, 'Regular', 6.49
FROM Products 
WHERE product_category = 'Wrap';

INSERT INTO Product_Sizes
SELECT product_id, 'Regular', 2.49
FROM Products 
WHERE product_name = 'Chips';

INSERT INTO Product_Sizes
SELECT product_id, 'Large', 3.49
FROM Products 
WHERE product_name = 'Chips';

INSERT INTO Product_Sizes
SELECT product_id, 'Regular', 4.49
FROM Products 
WHERE product_name = 'Cheesey Chips';

INSERT INTO Product_Sizes
SELECT product_id, '4pc', 3.99
FROM Products 
WHERE product_name = 'Garlic Bread';

INSERT INTO Product_Sizes
SELECT product_id, '6', 5.49
FROM Products 
WHERE product_name = 'Chicken Wings';

INSERT INTO Product_Sizes
SELECT product_id, '8', 3.49
FROM Products 
WHERE product_name = 'Onion Rings';

INSERT INTO Product_Sizes
SELECT product_id, '60ml', 0.7
FROM Products 
WHERE product_category = 'Dip';

INSERT INTO Product_Sizes
SELECT product_id, '330ml', 1.2
FROM Products 
WHERE product_category = 'Drink';

INSERT INTO Product_Sizes
SELECT product_id, '1.25l', 2.99
FROM Products 
WHERE product_category = 'Drink' AND product_name != 'Fanta';

INSERT INTO Toppings 
VALUES ('Pepperoni', 1.5, 1.8, 2.1),
('Ham', 1.5, 1.8, 2.1),
('Doner', 1.5, 1.8, 2.1),
('Sausage', 1.5, 1.8, 2.1),
('Chicken', 1.5, 1.8, 2.1),
('Beef', 1.5, 1.8, 2.1),
('Bacon', 1.5, 1.8, 2.1),
('Onion', 1, 1.2, 1.4),
('Pepper', 1, 1.2, 1.4),
('Tomato', 1, 1.2, 1.4),
('Pineapple', 1, 1.2, 1.4),
('Olive', 1, 1.2, 1.4),
('Cucumber', 1, 1.2, 1.4),
('Pickled Chilli', 1, 1.2, 1.4),
('BBQ', NULL, NULL, NULL),
('Lettuce', NULL, NULL, NULL),
('Pitta', NULL, NULL, NULL),
('Naan', NULL, NULL, NULL);

INSERT INTO Product_Toppings
SELECT P.product_id, T.topping_id
FROM Products AS P CROSS JOIN Toppings AS T
WHERE P.product_category = 'Pizza' AND T.topping_name NOT IN ('Lettuce', 'Pitta', 'Naan');

INSERT INTO Product_Toppings
SELECT P.product_id, T.topping_id
FROM Products AS P CROSS JOIN Toppings AS T
WHERE P.product_category = 'Kebab' 
AND T.topping_name IN ('Lettuce', 'Tomato', 'Onion', 'Cucmber', 'Pickled Chilli', 'Pitta', 'Naan'); 

INSERT INTO Allergens (allergen_description)
VALUES ('Celery'), ('Gluten'), ('Shellfish'), ('Egg'), ('Fish'), ('Lupin'), ('Dairy'), ('Mollusc'), 
	   ('Mustard'), ('Nuts'), ('Peanuts'), ('Sesame'), ('Soya'), ('Sulphites');

INSERT INTO Product_Allergens
SELECT P.product_id, A.allergen_id
FROM Products AS P CROSS JOIN Allergens AS A
WHERE P.product_category = 'Pizza'
AND A.allergen_description IN ('Gluten', 'Dairy', 'Soya', 'Sulphites', 'Celery');

INSERT INTO Product_Allergens
SELECT P.product_id, A.allergen_id
FROM Products AS P CROSS JOIN Allergens AS A
WHERE P.product_category = 'Burger'
AND A.allergen_description IN ('Gluten', 'Dairy', 'Soya', 'Egg', 'Sesame', 'Celery', 'Mustard');

INSERT INTO Product_Allergens
SELECT P.product_id, A.allergen_id
FROM Products AS P CROSS JOIN Allergens AS A
WHERE P.product_category = 'Wrap'
AND A.allergen_description IN ('Gluten', 'Dairy', 'Soya', 'Egg', 'Celery', 'Mustard');

INSERT INTO Product_Allergens
SELECT P.product_id, A.allergen_id
FROM Products AS P CROSS JOIN Allergens AS A
WHERE P.product_category = 'Kebab'
AND A.allergen_description IN ('Gluten', 'Dairy', 'Soya', 'Egg', 'Sesame', 'Celery', 'Mustard');

INSERT INTO Product_Allergens
SELECT P.product_id, A.allergen_id
FROM Products AS P CROSS JOIN Allergens AS A
WHERE P.product_name LIKE 'BBQ%'
AND A.allergen_description IN ('Fish', 'Mustard');

INSERT INTO Product_Allergens
SELECT P.product_id, A.allergen_id
FROM Products AS P CROSS JOIN Allergens AS A
WHERE P.product_name IN ('Cheesey Chips', 'Garlic Bread')
AND A.allergen_description IN ('Dairy', 'Soya');

INSERT INTO Product_Allergens
SELECT P.product_id, A.allergen_id
FROM Products AS P CROSS JOIN Allergens AS A
WHERE P.product_name IN ('Ketchup', 'Burger Sauce', 'Chilli')
AND A.allergen_description = 'Celery';

INSERT INTO Product_Allergens
SELECT P.product_id, A.allergen_id
FROM Products AS P CROSS JOIN Allergens AS A
WHERE P.product_name IN ('Mayo', 'Burger Sauce')
AND A.allergen_description IN ('Egg', 'Dairy', 'Mustard', 'Soya', 'Sulphites');

INSERT INTO Product_Allergens
SELECT P.product_id, A.allergen_id
FROM Products AS P CROSS JOIN Allergens AS A
WHERE P.product_name LIKE 'Cheese%'
AND A.allergen_description = 'Sulphites';

INSERT INTO Product_Allergens
SELECT P.product_id, A.allergen_id
FROM Products AS P CROSS JOIN Allergens AS A
WHERE P.product_name = 'Chicken Wings'
AND A.allergen_description = 'Soya';

INSERT INTO Product_Allergens
SELECT P.product_id, A.allergen_id
FROM Products AS P CROSS JOIN Allergens AS A
WHERE P.product_name = 'Garlic Bread'
AND A.allergen_description = 'Gluten';

INSERT INTO Topping_Allergens
SELECT T.topping_id, A.allergen_id
FROM Toppings AS T CROSS JOIN Allergens AS A
WHERE T.topping_name IN ('Sausage', 'Pitta', 'Naan')
AND A.allergen_description = 'Gluten';

INSERT INTO Topping_Allergens
SELECT T.topping_id, A.allergen_id
FROM Toppings AS T CROSS JOIN Allergens AS A
WHERE T.topping_name IN ('Sausage', 'BBQ', 'Pickled Chilli')
AND A.allergen_description = 'Sulphites';

INSERT INTO Topping_Allergens
SELECT T.topping_id, A.allergen_id
FROM Toppings AS T CROSS JOIN Allergens AS A
WHERE T.topping_name = 'BBQ'
AND A.allergen_description IN ('Fish', 'Mustard');

INSERT INTO Meal_Deals
VALUES ('Margherita, Chips & Drink', 10.99),
('Large Pizza & Dips', 12.99),
('Family Deal', 24.99),
('Kebab Meal', 9.49);

INSERT INTO Deal_Items
SELECT MD.deal_id, 'Pizza', 'Medium', 1, 
	(SELECT PS.size_id FROM Products AS P
    INNER JOIN Product_Sizes AS PS ON P.product_id = PS.product_id
    WHERE P.product_name = 'Margherita' AND PS.size_name = 'Medium') 
FROM Meal_Deals AS MD
WHERE MD.deal_name LIKE 'Margherita%';


INSERT INTO Deal_Items
SELECT MD.deal_id, 'Chips', 'Regular', 1, 
	(SELECT PS.size_id FROM Products AS P 
     INNER JOIN Product_Sizes AS PS ON P.product_id = PS.product_id
     WHERE P.product_name = 'Chips' AND size_name = 'Regular')
FROM Meal_Deals AS MD 
WHERE MD.deal_name LIKE 'Margherita%';

INSERT INTO Deal_Items (deal_id, product_category, size_name, quantity)
SELECT deal_id, 'Drink', '330ml', 1
FROM Meal_Deals 
WHERE deal_name LIKE 'Margherita%';

INSERT INTO Deal_Items (deal_id, product_category, size_name, quantity)
SELECT deal_id, 'Pizza', 'Large', 1
FROM Meal_Deals
WHERE deal_name LIKE 'Large Pizza%';

INSERT INTO Deal_Items (deal_id, product_category, size_name, quantity)
SELECT deal_id, 'Dip', '60ml', 2
FROM Meal_Deals
WHERE deal_name LIKE 'Large Pizza%';

INSERT INTO Deal_Items (deal_id, product_category, size_name, quantity)
SELECT deal_id, 'Pizza', 'Large', 2
FROM Meal_Deals
WHERE deal_name LIKE 'Family%';

INSERT INTO Deal_Items
SELECT MD.deal_id, 'Side', 'Large', 1, 
	(SELECT PS.size_id FROM Products AS P
    INNER JOIN Product_Sizes AS PS ON P.product_id = PS.product_id
    WHERE P.product_name = 'Chips' AND PS.size_name = 'Large') 
FROM Meal_Deals AS MD
WHERE MD.deal_name LIKE 'Family%%';

INSERT INTO Deal_Items (deal_id, product_category, size_name, quantity)
SELECT deal_id, 'Drink', '1.25l', 1
FROM Meal_Deals
WHERE deal_name LIKE 'Family%';

INSERT INTO Deal_Items
SELECT MD.deal_id, 'Kebab', 'Regular', 1, 
	(SELECT PS.size_id FROM Products AS P
    INNER JOIN Product_Sizes AS PS ON P.product_id = PS.product_id
    WHERE P.product_name = 'Doner Kebab' AND PS.size_name = 'Regular') 
FROM Meal_Deals AS MD
WHERE MD.deal_name LIKE 'Kebab%';

INSERT INTO Deal_Items
SELECT MD.deal_id, 'Side', 'Regular', 1, 
	(SELECT PS.size_id FROM Products AS P
    INNER JOIN Product_Sizes AS PS ON P.product_id = PS.product_id
    WHERE P.product_name = 'Chips' AND PS.size_name = 'Regular') 
FROM Meal_Deals AS MD
WHERE MD.deal_name LIKE 'Kebab%';

INSERT INTO Deal_Items (deal_id, product_category, size_name, quantity)
SELECT deal_id, 'Drink', '330ml', 1
FROM Meal_Deals
WHERE deal_name LIKE 'Kebab%';

INSERT INTO Order_Statuses (status_name)
VALUES ('New'), ('Preparing'), ('Ready'), ('Out for delivery'), ('Completed'), ('Cancelled'), ('Refunded')

