INSERT INTO Users (name, PIN, role)
VALUES 
('Brent', 'xgAdWyrD3zFCBKj516AOFQPJq6D9RThkXeS/TMfiVVz+n/nQI2vzJ+0+kHhJqY300zDEvqVRAX1GW0wdm4C8sA==', 'Manager'),
('Alice', 'ZcXG3TS/LG7ymRvKTaf4C7lUgLMO3MVHIhhQ74DRZ9Y6opYN1hADfRhn4SA+eXs5kbweoYtXXeAVLyo8XD1ioQ==', 'Cashier'),
('Luigi', 'WelDWxnS1ppbca30sPGWL2EqPDIp2AyC396BbqSk51g2MDCWit9P5+v/auYLk+SkaCposGHgjhfb6FlHqdPXNQ==', 'Pizzaiolo'),
('Bob', 'jDOx5TJH6KvflW296AOIc5ur1hXzDkKc3Q65ru371NgOZYIIi7A0h4N7fWt4SonJLT0QAzRjgjgt5o0OfWR2vQ==', 'Grill Cook'),
('Bill', 'PMgv1CLtaQnylV658I29pYU6fYTRlzwlMVuSprJWxlD49cLVtVg4LhKKAzIPSzqyi4ilJehhNwxI79XSti0a6g==', 'Driver'),
('John', 'RFh5fnWYpzzJvAFxjQfqJhxzFM03bLhpxSlSj78SmEVXRAcK3/ptoFuQSnBNKIEItq7QAC23ne6wQ/YD/8TFgQ==', 'Driver');

INSERT INTO Customers (name, phone_no, post_code, house_no, street_address)
VALUES ('Ali Wong', '01234 555111', 'TA6 4AB', 10, 'Oak Road'),
('Samir Patel', '01234 555222', 'TA6 5CD', 22, 'Bridge Street'),
('Jess Brown', '01234 555333', 'TA6 1EF', 7, 'Mill Close');

INSERT INTO Delivery_Fees
VALUES (2, 2.00), (4, 3.00);

INSERT INTO Promotions (promo_code, description, discount_value, min_spend)
VALUES ('PIZZA10', '10% off when you spend £15', 0.1, 15.0),
(NULL, 'Student Discount', 0.1, 12.0);

INSERT INTO Products (product_name, product_category)
VALUES ('Margherita', 'Pizza'),
('Pepperoni', 'Pizza'),
('Hawaiian', 'Pizza'),
('Veggie Supreme', 'Pizza'),
('BBQ Chicken', 'Pizza'),
('Meat Feast', 'Pizza'),
('Doner Delight', 'Pizza'), 
('Build-Your-Own', 'Pizza'),
('Doner Kebab', 'Kebab'),
('Chicken Shish', 'Kebab'),
('Mixed Kebab', 'Kebab'),
('Kebab Box', 'Kebab'),
('Cheese Burger', 'Burger'),
('Chicken Fillet Burger', 'Burger'),
('Doner Wrap', 'Wrap'),
('Chicken Wrap', 'Wrap'),
('Chips', 'Side'),
('Cheesey Chips', 'Side'),
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

INSERT INTO Sizes (size_name) 
VALUES ('Small'), ('Medium'), ('Large'), ('Regular'), ('330ml'), ('1.25l'), ('4'), ('6'), ('8'), ('60ml');

INSERT INTO Topping_Types (topping_type)
VALUES ('Meat'), ('Veg'), ('Base'), ('Kebab'), ('Bread');

INSERT INTO Toppings 
Select T.ToppingName, TT.topping_type_id
FROM
(VALUES ('Pepperoni'),
('Ham'),
('Doner'),
('Sausage'),
('Chicken'),
('Beef'),
('Bacon')) AS T(ToppingName)
CROSS JOIN Topping_Types AS TT
WHERE TT.topping_type = 'Meat';

INSERT INTO Toppings
SELECT T.ToppingName, TT.topping_type_id
FROM
(Values ('Onion'),
('Pepper'),
('Tomato'),
('Pineapple'),
('Olive'),
('Mushroom'),
('Sweetcorn')) AS T(ToppingName)
CROSS JOIN Topping_Types AS TT
WHERE TT.topping_type = 'Veg';

INSERT INTO Toppings
SELECT T.ToppingName, TT.topping_type_id
FROM
(Values ('Tomato'), ('BBQ')) AS T(ToppingName) 
CROSS JOIN Topping_Types AS TT 
WHERE TT.topping_type = 'Base';

INSERT INTO Toppings
SELECT T.ToppingName, TT.topping_type_id
FROM
(VALUES ('Onion'),
('Tomato'),
('Cucumber'),
('Pickled Chilli'),
('Lettuce')) AS T(ToppingName)
CROSS JOIN Topping_Types AS TT 
WHERE TT.topping_type = 'Kebab';

INSERT INTO Toppings
SELECT T.ToppingName, TT.topping_type_id
FRom
(Values ('Pitta'), ('Naan')) AS T(ToppingName)
CROSS JOIN Topping_Types as TT
WHERE TT.topping_type = 'Bread';

INSERT INTO Product_Toppings
SELECT P.product_id, TT.topping_type_id
FROM Products AS P CROSS JOIN Topping_Types AS TT
WHERE P.product_category = 'Pizza' AND TT.topping_type = 'Base';

INSERT INTO Product_Toppings
SELECT P.product_id, TT.topping_type_id
FROM Products AS P CROSS JOIN Topping_Types AS TT
WHERE P.product_name IN ('Doner Kebab', 'Mixed Kebab') 
AND TT.topping_type = 'Bread'; 

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
WHERE P.product_name IN ('Garlic Bread', 'Onion Rings')
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

INSERT INTO Topping_Prices
SELECT
    (SELECT TT.topping_type_id FROM Topping_Types AS TT WHERE TT.topping_type = 'Veg'), 
    (SELECT S.size_id FROM Sizes AS S WHERE S.size_name = 'Small'), 
    1.00;

INSERT INTO Topping_Prices
SELECT
    (SELECT TT.topping_type_id FROM Topping_Types AS TT WHERE TT.topping_type = 'Veg'), 
    (SELECT S.size_id FROM Sizes AS S WHERE S.size_name = 'Medium'), 
    1.20;

INSERT INTO Topping_Prices
SELECT
    (SELECT TT.topping_type_id FROM Topping_Types AS TT WHERE TT.topping_type = 'Veg'), 
    (SELECT S.size_id FROM Sizes AS S WHERE S.size_name = 'Large'), 
    1.40;

INSERT INTO Topping_Prices
SELECT
    (SELECT TT.topping_type_id FROM Topping_Types AS TT WHERE TT.topping_type = 'Meat'), 
    (SELECT S.size_id FROM Sizes AS S WHERE S.size_name = 'Small'), 
    1.50;

INSERT INTO Topping_Prices
SELECT
    (SELECT TT.topping_type_id FROM Topping_Types AS TT WHERE TT.topping_type = 'Meat'), 
    (SELECT S.size_id FROM Sizes AS S WHERE S.size_name = 'Medium'), 
    1.80;

INSERT INTO Topping_Prices
SELECT
    (SELECT TT.topping_type_id FROM Topping_Types AS TT WHERE TT.topping_type = 'Meat'), 
    (SELECT S.size_id FROM Sizes AS S WHERE S.size_name = 'Large'), 
    2.10;

INSERT INTO Topping_Prices
SELECT TT.topping_type_id, S.size_id, 0.0
FROM Topping_Types AS TT 
CROSS JOIN Sizes AS S
WHERE TT.topping_type IN ('Kebab', 'Bread')
AND S.size_name IN ('Regular', 'Large');

INSERT INTO Topping_Prices
SELECT TT.topping_type_id, S.size_id, 0.0
FROM Topping_Types AS TT
CROSS JOIN Sizes AS S
WHERE TT.topping_type = 'Base'
AND S.size_name IN ('Small', 'Medium', 'Large');

INSERT INTO Product_Prices
SELECT (SELECT P.product_id FROM Products AS P WHERE P.product_name = 'Margherita'), 
(SELECT S.size_id FROM Sizes AS S WHERE S.size_name = 'Small'), 6.99;

INSERT INTO Product_Prices
SELECT (SELECT P.product_id FROM Products AS P WHERE P.product_name = 'Margherita'), 
(SELECT S.size_id FROM Sizes AS S WHERE S.size_name = 'Medium'), 8.99;

INSERT INTO Product_Prices
SELECT (SELECT P.product_id FROM Products AS P WHERE P.product_name = 'Margherita'), 
(SELECT S.size_id FROM Sizes AS S WHERE S.size_name = 'Large'), 11.49;

INSERT INTO Product_Prices
SELECT P.product_id, S.size_id, 7.99
FROM Products AS P
CROSS JOIN Sizes AS S
WHERE P.product_name IN ('Pepperoni', 'Hawaiian', 'Veggie Supreme')
AND S.size_name = 'Small';

INSERT INTO Product_Prices
SELECT P.product_id, S.size_id, 9.99
FROM Products AS P
CROSS JOIN Sizes AS S
WHERE P.product_name IN ('Pepperoni', 'Hawaiian', 'Veggie Supreme')
AND S.size_name = 'Medium';

INSERT INTO Product_Prices
SELECT P.product_id, S.size_id, 12.99
FROM Products AS P
CROSS JOIN Sizes AS S
WHERE P.product_name IN ('Pepperoni', 'Hawaiian', 'Veggie Supreme')
AND S.size_name = 'Large';

INSERT INTO Product_Prices
SELECT P.product_id, S.size_id, 8.49
FROM Products AS P
CROSS JOIN Sizes AS S
WHERE P.product_name = 'BBQ Chicken'
AND S.size_name = 'Small';

INSERT INTO Product_Prices
SELECT P.product_id, S.size_id, 8.99
FROM Products AS P
CROSS JOIN Sizes AS S
WHERE P.product_name IN ('Meat Feast', 'Doner Delight')
AND S.size_name = 'Small';

INSERT INTO Product_Prices
SELECT P.product_id, S.size_id, 10.49
FROM Products AS P
CROSS JOIN Sizes AS S
WHERE P.product_name = 'BBQ Chicken'
AND S.size_name = 'Medium';

INSERT INTO Product_Prices
SELECT P.product_id, S.size_id, 10.99
FROM Products AS P
CROSS JOIN Sizes AS S
WHERE P.product_name IN ('Meat Feast', 'Doner Delight')
AND S.size_name = 'Medium';

INSERT INTO Product_Prices
SELECT P.product_id, S.size_id, 13.49
FROM Products AS P
CROSS JOIN Sizes AS S
WHERE P.product_name = 'BBQ Chicken'
AND S.size_name = 'Large';

INSERT INTO Product_Prices
SELECT P.product_id, S.size_id, 13.99
FROM Products AS P
CROSS JOIN Sizes AS S
WHERE P.product_name IN ('Meat Feast', 'Doner Delight')
AND S.size_name = 'Large';

INSERT INTO Product_Prices
SELECT P.product_id, S.size_id, 6.49
FROM Products AS P
CROSS JOIN Sizes AS S
WHERE P.product_name = 'Build-Your-Own'
AND S.size_name = 'Small';

INSERT INTO Product_Prices
SELECT P.product_id, S.size_id, 8.49
FROM Products AS P
CROSS JOIN Sizes AS S
WHERE P.product_name = 'Build-Your-Own'
AND S.size_name = 'Medium';

INSERT INTO Product_Prices
SELECT P.product_id, S.size_id, 10.99
FROM Products AS P
CROSS JOIN Sizes AS S
WHERE P.product_name = 'Build-Your-Own'
AND S.size_name = 'Large';

INSERT INTO Product_Prices
SELECT P.product_id, S.size_id, 6.49
FROM Products AS P
CROSS JOIN Sizes AS S
WHERE P.product_name = 'Doner Kebab'
AND S.size_name = 'Regular';

INSERT INTO Product_Prices
SELECT P.product_id, S.size_id, 7.99
FROM Products AS P
CROSS JOIN Sizes AS S
WHERE P.product_name = 'Doner Kebab'
AND S.size_name = 'Large';

INSERT INTO Product_Prices
SELECT P.product_id, S.size_id, 7.49
FROM Products AS P
CROSS JOIN Sizes AS S
WHERE P.product_name = 'Chicken Shish'
AND S.size_name = 'Regular';

INSERT INTO Product_Prices
SELECT P.product_id, S.size_id, 8.99
FROM Products AS P
CROSS JOIN Sizes AS S
WHERE P.product_name = 'Chicken Shish'
AND S.size_name = 'Large';

INSERT INTO Product_Prices
SELECT P.product_id, S.size_id, 9.49
FROM Products AS P
CROSS JOIN Sizes AS S
WHERE P.product_name = 'Mixed Kebab'
AND S.size_name = 'Regular';

INSERT INTO Product_Prices
SELECT P.product_id, S.size_id, 7.99
FROM Products AS P
CROSS JOIN Sizes AS S
WHERE P.product_name = 'Kebab Box'
AND S.size_name = 'Regular';

INSERT INTO Product_Prices
SELECT P.product_id, S.size_id, 5.49
FROM Products AS P
CROSS JOIN Sizes AS S
WHERE P.product_name = 'Cheese Burger'
AND S.size_name = 'Regular';

INSERT INTO Product_Prices
SELECT P.product_id, S.size_id, 5.99
FROM Products AS P
CROSS JOIN Sizes AS S
WHERE P.product_name = 'Chicken Fillet Burger'
AND S.size_name = 'Regular';

INSERT INTO Product_Prices
SELECT P.product_id, S.size_id, 6.49
FROM Products AS P
CROSS JOIN Sizes AS S
WHERE P.product_name LIKE '%Wrap'
AND S.size_name = 'Regular';

INSERT INTO Product_Prices
SELECT P.product_id, S.size_id, 2.49
FROM Products AS P
CROSS JOIN Sizes AS S
WHERE P.product_name = 'Chips'
AND S.size_name = 'Regular';

INSERT INTO Product_Prices
SELECT P.product_id, S.size_id, 3.49
FROM Products AS P
CROSS JOIN Sizes AS S
WHERE P.product_name = 'Chips'
AND S.size_name = 'Large';

INSERT INTO Product_Prices
SELECT P.product_id, S.size_id, 4.49
FROM Products AS P
CROSS JOIN Sizes AS S
WHERE P.product_name = 'Cheesey Chips'
AND S.size_name = 'Regular';

INSERT INTO Product_Prices
SELECT P.product_id, S.size_id, 2.49
FROM Products AS P
CROSS JOIN Sizes AS S
WHERE P.product_name = 'Garlic Bread'
AND S.size_name = '4';

INSERT INTO Product_Prices
SELECT P.product_id, S.size_id, 5.49
FROM Products AS P
CROSS JOIN Sizes AS S
WHERE P.product_name = 'Chicken Wings'
AND S.size_name = '6';

INSERT INTO Product_Prices
SELECT P.product_id, S.size_id, 3.49
FROM Products AS P
CROSS JOIN Sizes AS S
WHERE P.product_name = 'Onion Rings'
AND S.size_name = '8';

INSERT INTO Product_Prices
SELECT P.product_id, S.size_id, 0.7
FROM Products AS P
CROSS JOIN Sizes AS S
WHERE P.product_category = 'Dip'
AND S.size_name = '60ml';

INSERT INTO Product_Prices
SELECT P.product_id, S.size_id, 1.2
FROM Products AS P
CROSS JOIN Sizes AS S
WHERE P.product_category = 'Drink'
AND S.size_name = '330ml';

INSERT INTO Product_Prices
SELECT P.product_id, S.size_id, 2.99
FROM Products AS P
CROSS JOIN Sizes AS S
WHERE P.product_category = 'Drink'
AND P.product_name != 'Fanta'
AND S.size_name = '1.25l';

INSERT INTO Allowed_Product_Categories 
SELECT TT.topping_type_id, 'Kebab'
FROM Topping_Types as TT
WHERE TT.topping_type IN ('Bread', 'Kebab');

INSERT INTO Allowed_Product_Categories
SELECT TT.topping_type_id, 'Pizza'
FROM Topping_Types as TT
WHERE TT.topping_type IN ('Meat', 'Veg', 'Base');

INSERT INTO Meal_Deals
VALUES ('Margherita, Chips & Drink', 10.99),
('Large Pizza & Dips', 12.99),
('Family Deal', 24.99),
('Kebab Meal', 9.49);

INSERT INTO Deal_Items 
SELECT MD.deal_id, 
    (SELECT P.product_id FROM Products AS P WHERE P.product_name = 'Margherita'),
    'Pizza', 
    (SELECT S.size_id FROM Sizes AS S WHERE S.size_name = 'Medium'), 1
FROM Meal_Deals AS MD
WHERE MD.deal_name LIKE 'Margherita%';


INSERT INTO Deal_Items
SELECT MD.deal_id, 
    (SELECT P.product_id FROM Products AS P WHERE P.product_name = 'Chips'), 
    'Side', 
	(SELECT S.size_id FROM Sizes AS S WHERE S.size_name = 'Regular'), 1
FROM Meal_Deals AS MD 
WHERE MD.deal_name LIKE 'Margherita%';

INSERT INTO Deal_Items (deal_id, product_category, size_id, quantity)
SELECT MD.deal_id, 'Drink', (SELECT S.size_id FROM Sizes AS S WHERE S.size_name = '330ml'), 1
FROM Meal_Deals AS MD
WHERE MD.deal_name LIKE 'Margherita%';

INSERT INTO Deal_Items (deal_id, product_category, size_id, quantity)
SELECT MD.deal_id, 'Pizza', (SELECT S.size_id FROM Sizes AS S WHERE S.size_name = 'Large'), 1
FROM Meal_Deals AS MD
WHERE MD.deal_name LIKE 'Large Pizza%';

INSERT INTO Deal_Items (deal_id, product_category, size_id, quantity)
SELECT MD.deal_id, 'Dip', (SELECT S.size_id FROM Sizes AS S WHERE S.size_name = '60ml'), 2
FROM Meal_Deals AS MD
WHERE MD.deal_name LIKE 'Large Pizza%';

INSERT INTO Deal_Items (deal_id, product_category, size_id, quantity)
SELECT MD.deal_id, 'Pizza', (SELECT S.size_id FROM Sizes AS S WHERE S.size_name = 'Large'), 2
FROM Meal_Deals AS MD
WHERE MD.deal_name LIKE 'Family%';

INSERT INTO Deal_Items
SELECT MD.deal_id, (SELECT P.product_id FROM Products AS P WHERE P.product_name = 'Chips'),
'Side', (SELECT S.size_id FROM Sizes AS S WHERE S.size_name = 'Large'), 1
FROM Meal_Deals AS MD
WHERE MD.deal_name LIKE 'Family%%';

INSERT INTO Deal_Items (deal_id, product_category, size_id, quantity)
SELECT MD.deal_id, 'Drink', (SELECT S.size_id FROM Sizes AS S WHERE S.size_name = '1.25l'), 1
FROM Meal_Deals AS MD
WHERE MD.deal_name LIKE 'Family%';

INSERT INTO Deal_Items
SELECT MD.deal_id, (SELECT P.product_id FROM Products AS P WHERE p.product_name = 'Doner Kebab'), 'Kebab', 
    (SELECT S.size_id FROM Sizes AS S WHERE S.size_name = 'Regular'), 1 	
FROM Meal_Deals AS MD
WHERE MD.deal_name LIKE 'Kebab%';

INSERT INTO Deal_Items
SELECT MD.deal_id, (SELECT P.product_id FROM Products AS P WHERE P.product_name = 'Chips'), 'Side', 
    (SELECT S.size_id FROM Sizes AS S WHERE S.size_name = 'Regular'), 1
FROM Meal_Deals AS MD
WHERE MD.deal_name LIKE 'Kebab%';

INSERT INTO Deal_Items (deal_id, product_category, size_id, quantity)
SELECT MD.deal_id, 'Drink', (SELECT S.size_id FROM Sizes AS S WHERE S.size_name = '330ml'), 1
FROM Meal_Deals AS MD
WHERE MD.deal_name LIKE 'Kebab%';

INSERT INTO Order_Status (status_name)
VALUES ('New'), ('Preparing'), ('Pizza Ready'), ('Grill Ready'), ('Order Ready'), ('Out for delivery'), ('Completed'), ('Cancelled'), ('Refunded')

INSERT INTO Opening_Times (day_id, day_name, open_time, close_time)
VALUES 
	(1, 'Monday', '12:00:00', '23:00:00'),
	(2, 'Tuesday', '12:00:00', '23:00:00'),
	(3, 'Wednesday', '12:00:00', '23:00:00'),
	(4, 'Thursday', '12:00:00', '23:00:00'),
	(5, 'Friday', '12:00:00', '00:00:00'),
	(6, 'Saturday', '12:00:00', '00:00:00'),
	(7, 'Sunday', '12:00:00', '22:00:00');