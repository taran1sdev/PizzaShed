USE master;
GO

IF DB_ID(N'PizzaShed') IS NOT NULL
    DROP DATABASE PizzaShed;
GO

CREATE DATABASE PizzaShed;
GO

USE PizzaShed;


IF NOT EXISTS
    (SELECT name 
    FROM sys.database_principals
    WHERE name = 'PizzaShedDB')
BEGIN
    CREATE LOGIN PizzaShedDB
	    WITH PASSWORD = 'PizzaShedDBPassword';
    CREATE USER PizzaShedDB FOR LOGIN PizzaShedDB;
END

DROP TABLE IF EXISTS Order_Product_Toppings;
DROP TABLE IF EXISTS Product_Allergens;
DROP TABLE IF EXISTS Order_Products;
DROP TABLE IF EXISTS Product_Toppings;
DROP TABLE IF EXISTS Topping_Allergens;
DROP TABLE IF EXISTS Topping_Prices;
DROP TABLE IF EXISTS Toppings;
DROP TABLE IF EXISTS Allergens;
DROP TABLE IF EXISTS Deal_Items;
DROP TABLE IF EXISTS Meal_Deals;
DROP TABLE IF EXISTS Order_Payments;
DROP TABLE IF EXISTS Orders;
DROP TABLE IF EXISTS Order_Status;
DROP TABLE IF EXISTS Product_Prices;
DROP TABLE IF EXISTS Products;
DROP TABLE IF EXISTS Users;
DROP TABLE IF EXISTS Customers;
DROP TABLE IF EXISTS Promotions;
DROP TABLE IF EXISTS Drivers;
DROP TABLE IF EXISTS Sizes;
DROP TABLE IF EXISTS Allowed_Product_Categories;
DROP TABLE IF EXISTS Topping_Types;
DROP TABLE IF EXISTS Opening_Times;
DROP TABLE IF EXISTS Delivery_Fees;
DROP TABLE IF EXISTS Payments;
DROP TYPE IF EXISTS ProductListType;
DROP TYPE IF EXISTS ToppingListType;

CREATE TABLE Users (
  user_id int IDENTITY(1,1),
  name varchar(255) NOT NULL,
  PIN varchar(255) NOT NULL,
  role varchar(32) NOT NULL,
  
  PRIMARY KEY (user_id)
);

ALTER TABLE Users ADD CONSTRAINT U_PIN UNIQUE(PIN);

CREATE TABLE Customers (
  customer_id int IDENTITY(1,1),
  name varchar(64) NOT NULL,
  phone_no varchar(13) NOT NULL,
  post_code varchar(8) not NULL,
  flat_no varchar(8),
  house_no int NOT NULL,
  street_address varchar(128) NOT NULL,
  delivery_notes varchar(255)  
  
  PRIMARY KEY (customer_id)
);

ALTER TABLE Customers ADD CONSTRAINT U_PHONE_NO UNIQUE(phone_no);


CREATE TABLE Order_Status (
  order_status_id int IDENTITY(1,1),
  status_name varchar(16) NOT NULL,
  
  PRIMARY KEY(order_status_id)
);

CREATE TABLE Drivers (
  driver_id int IDENTITY(1,1),
  name varchar(16) NOT NULL,
  current_status varchar NOT NULL,
  
  PRIMARY KEY(driver_id)
);

CREATE TABLE Delivery_Fees (
	max_distance int,
	price smallmoney,

	PRIMARY KEY(max_distance)
);

CREATE TABLE Promotions (
  promo_id int IDENTITY(1,1),
  promo_code varchar(16),
  description varchar(64) NOT NULL,
  discount_value smallmoney NOT NULL,
  min_spend smallmoney not NULL,
  
  PRIMARY KEY(promo_id)
);

CREATE TABLE Products (
  product_id int IDENTITY(1,1),
  product_name varchar(64) NOT NULL,
  product_category varchar(16) NOT NULL,
  
  PRIMARY KEY(product_id)
);

CREATE TABLE Sizes (
	size_id int IDENTITY(1,1),
	size_name varchar(32),

	PRIMARY KEY(size_id)
)


CREATE TABLE Toppings (
  topping_id int IDENTITY(1,1),
  topping_name varchar(32) NOT NULL,
  topping_type_id int,

  PRIMARY KEY(topping_id)
);

CREATE TABLE Topping_Types (
	topping_type_id int IDENTITY(1,1),
	topping_type varchar(32),
	
	PRIMARY KEY(topping_type_id)
);

ALTER TABLE Toppings 
	ADD CONSTRAINT FK_Toppings_ToppingTypes FOREIGN KEY (topping_type_id) 
	REFERENCES Topping_Types (topping_type_id);


CREATE TABLE Topping_Prices (	
	topping_type_id integer,
	size_id int,
	price smallmoney

	FOREIGN KEY(topping_type_id) REFERENCES Topping_Types (topping_type_id),
	FOREIGN KEY(size_id) REFERENCES Sizes (size_id),	
	PRIMARY KEY(size_id, topping_type_id)
);

CREATE TABLE Product_Prices (
	product_id int,
	size_id int,
	price smallmoney,

	FOREIGN KEY(product_id) REFERENCES Products (product_id),
	FOREIGN KEY(size_id) REFERENCES Sizes (size_id),
	PRIMARY KEY (product_id, size_id)
);


CREATE TABLE Product_Toppings (
  product_id int,
  topping_type_id int,
  
  FOREIGN KEY(product_id) REFERENCES Products (product_id),
  FOREIGN KEY(topping_type_id) REFERENCES Topping_Types (topping_type_id),
  PRIMARY KEY(product_id,topping_type_id)
);

CREATE TABLE Allergens (
  allergen_id int IDENTITY(1,1),
  allergen_description varchar(64) NOT NULL,
  
  PRIMARY KEY(allergen_id)
);

CREATE TABLE Product_Allergens (
  product_id int,
  allergen_id int,
  
  FOREIGN KEY(product_id) REFERENCES Products (product_id),
  FOREIGN KEY(allergen_id) REFERENCES Allergens (allergen_id),
  PRIMARY KEY(product_id,allergen_id)
);

CREATE TABLE Topping_Allergens (
  topping_id int,
  allergen_id int,
  
  FOREIGN KEY(topping_id) REFERENCES Toppings (topping_id),
  FOREIGN KEY(allergen_id) REFERENCES Allergens (allergen_id),
  PRIMARY KEY(topping_id,allergen_id)
);

CREATE TABLE Allowed_Product_Categories (
	topping_type_id int,
	product_category varchar(32)

	FOREIGN KEY(topping_type_id) REFERENCES Topping_Types (topping_type_id),
	PRIMARY KEY (topping_type_id, product_category)
);

CREATE TABLE Meal_Deals (
  deal_id int IDENTITY(1,1),
  deal_name varchar(64) NOT NULL,
  price smallmoney NOT NULL,
  
  PRIMARY KEY(deal_id)
);

CREATE TABLE Deal_Items (
  deal_item_id int IDENTITY(1,1),
  deal_id int,
  product_id int,
  product_category varchar(16) NOT NULL,  
  size_id int,
  quantity int,
  
  FOREIGN KEY(product_id) REFERENCES Products (product_id),
  FOREIGN KEY(deal_id) REFERENCES Meal_Deals (deal_id),
  FOREIGN KEY(size_id) REFERENCES Sizes (size_id),
  PRIMARY KEY(deal_item_id)
);

CREATE TABLE Orders (
  order_id int IDENTITY(1,1),
  user_id  int NOT NULL,
  customer_id int,
  order_status_id int NOT NULL,
  pizza_ready bit,
  grill_ready bit,
  order_date datetime NOT NULL,
  collection_time datetime,
  order_source varchar(8),
  order_notes varchar(255),
  order_type varchar(16) NOT NULL,
  driver_id int,
  paid bit NOT NULL,  
  total_price smallmoney NOT NULL,
  delivery_fee smallmoney,
  promo_id int,
  
  FOREIGN KEY(user_id) REFERENCES Users (user_id),
  FOREIGN KEY(customer_id) REFERENCES Customers (customer_id),
  FOREIGN KEY(order_status_id) REFERENCES Order_Status (order_status_id),
  FOREIGN KEY(driver_id) REFERENCES Drivers (driver_id),
  FOREIGN KEY(promo_id) REFERENCES Promotions (promo_id),
  PRIMARY KEY(order_id)
);

CREATE TABLE Payments (
	payment_id int IDENTITY(1,1),
	payment_type varchar(50),
	amount smallmoney,

	PRIMARY KEY(payment_id)
);

CREATE TABLE Order_Payments (
	payment_id int,
	order_id int,

	FOREIGN KEY(payment_id) REFERENCES Payments (payment_id),
	FOREIGN KEY(order_id) REFERENCES Orders (order_id),
	PRIMARY KEY(payment_id, order_id)
);

CREATE TABLE Order_Products (
  order_product_id int IDENTITY(1,1),
  order_id int NOT NULL,
  product_id int,
  second_half_id int NULL,
  size_id int,
  deal_id int,
  deal_instance_id int,
  
  FOREIGN KEY(order_id) REFERENCES Orders (order_id),
  FOREIGN KEY(product_id) REFERENCES Products (product_id),
  FOREIGN KEY(product_id) REFERENCES Products (product_id),
  FOREIGN KEY(size_id) REFERENCES Sizes(size_id),
  FOREIGN KEY(deal_id) REFERENCES Meal_Deals (deal_id),  
  PRIMARY KEY(order_product_id)
);

CREATE TABLE Order_Product_Toppings (
  order_product_id int,
  topping_id int,
  
  FOREIGN KEY(order_product_id) REFERENCES Order_Products (order_product_id),
  FOREIGN KEY(topping_id) REFERENCES Toppings(topping_id),
  PRIMARY KEY(order_product_id,topping_id)
);

CREATE TABLE Opening_Times (
	day_id INT,
	day_name VARCHAR(10),
	open_time TIME(0),
	close_time TIME(0)
	
	PRIMARY KEY(day_id)
);

CREATE TYPE ProductListType AS Table (
	product_id INT,
	second_half_id INT NULL,
	size_name VARCHAR(30),
	deal_id INT NULL,
	deal_instance_id INT NULL,
	client_product_id INT
);

CREATE TYPE ToppingListType AS TABLE (
	client_product_id INT,
	topping_id INT
);

IF OBJECT_ID('dbo.DeleteOrder', 'P') IS NOT NULL
    DROP PROCEDURE DeleteOrder
GO

CREATE PROCEDURE DeleteOrder
    @orderID INT
AS
BEGIN
    SET NOCOUNT ON;  

    BEGIN TRY

    BEGIN TRANSACTION;       
    
    DELETE FROM Order_Product_Toppings
    WHERE order_product_id IN (
        SELECT order_product_id
        FROM Order_Products
        WHERE order_id = @orderID
    );
    
    DELETE FROM Order_Products 
    WHERE order_id = @orderID;

    DELETE FROM Orders 
    WHERE order_id = @orderID;

    IF @@ERROR = 0
        COMMIT TRANSACTION;
    END TRY        
    BEGIN CATCH
        ROLLBACK TRANSACTION; 
        THROW;
    END CATCH
END
GO

IF OBJECT_ID('dbo.CreateOrder', 'P') IS NOT NULL
    DROP PROCEDURE CreateOrder
GO

CREATE PROCEDURE CreateOrder
    @userID INT,
    @orderType VARCHAR(12),
    @price SMALLMONEY,
    @ProductList AS ProductListType READONLY,
    @ToppingList AS ToppingListType READONLY
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @orderID INT;
    DECLARE @statusID INT;

    SELECT @statusID = order_status_id FROM Order_Status WHERE status_name = 'New';

    BEGIN TRANSACTION;
    
    INSERT INTO Orders(user_id, order_status_id, order_date, order_type, paid, total_price)    
    VALUES (
        @userID,
        @statusID,
        GETDATE(),
        @orderType,
        0,
        @price);

    SET @orderID = SCOPE_IDENTITY();
    
    IF @orderID IS NULL OR @orderID = 0
    BEGIN
        ROLLBACK TRANSACTION;
        SELECT 0 AS NewOrderID;
        RETURN;
    END
    
    CREATE TABLE #Inserted_Order_Products (
        order_product_id INT,
        client_product_id INT  
    );

    -- If we use a regular INSERT we can't retrieve our temporary client_product_id to track the toppings for each product
    -- Using a MERGE statement allows us to match products and toppings in one transaction
    MERGE INTO Order_Products AS Target
    USING (
        SELECT 
            NULLIF(p.product_id,0) AS product_id,
            NULLIF(p.second_half_id,0) AS second_half_id,
            (SELECT s.size_id FROM Sizes AS s WHERE s.size_name = p.size_name) AS size_id,
            NULLIF(p.deal_id,0) AS deal_id,
            p.deal_instance_id,
            p.client_product_id
        FROM @ProductList AS p
    ) AS Source 
    ON 1 = 0 
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (order_id, product_id, second_half_id, size_id, deal_id, deal_instance_id)
        VALUES (
            @orderID, 
            Source.product_id, 
            Source.second_half_id,
            Source.size_id, 
            Source.deal_id, 
            Source.deal_instance_id
        )
    
    OUTPUT INSERTED.order_product_id, Source.client_product_id INTO #Inserted_Order_Products;

    
    INSERT INTO Order_Product_Toppings (order_product_id, topping_id) 
    SELECT 
        iop.order_product_id,
        t.topping_id
    FROM #Inserted_Order_Products AS iop
    INNER JOIN @ToppingList AS t ON iop.client_product_id = t.client_product_id;

    
    DROP TABLE #Inserted_Order_Products;

    
    IF @@ERROR = 0
        COMMIT TRANSACTION;
    ELSE
    BEGIN
        ROLLBACK TRANSACTION;
        SET @orderID = 0;
    END

    SELECT @orderID AS NewOrderID;
END
GO

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
VALUES ('New'), ('Preparing'), ('Order Ready'), ('Out For Delivery'), ('Completed'), ('Cancelled'), ('Refunded')

INSERT INTO Opening_Times (day_id, day_name, open_time, close_time)
VALUES 
	(1, 'Monday', '12:00:00', '23:00:00'),
	(2, 'Tuesday', '12:00:00', '23:00:00'),
	(3, 'Wednesday', '12:00:00', '23:00:00'),
	(4, 'Thursday', '12:00:00', '23:00:00'),
	(5, 'Friday', '12:00:00', '23:59:59'),
	(6, 'Saturday', '12:00:00', '23:59:59'),
	(7, 'Sunday', '12:00:00', '22:59:59');