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