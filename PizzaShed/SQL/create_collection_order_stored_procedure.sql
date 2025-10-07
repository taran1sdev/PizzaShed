CREATE PROCEDURE CreateCollectionOrder
	@userID INT,
	@price SMALLMONEY,
	@ProductList AS ProductListType READONLY,
	@ToppingList AS ToppingListType READONLY
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @orderID INT;

	BEGIN TRANSACTION;

	INSERT INTO Orders(user_id, order_status_id, order_date, order_type, paid, total_price)
	OUTPUT INSERTED.order_id
	VALUES (
		@userID,
		(SELECT os.order_status_id FROM Order_Status AS os WHERE os.status_name = 'New'),
		GETDATE(),
		'Collection',
		0,
		@price);

	SET @orderID = SCOPE_IDENTITY();

	CREATE TABLE #Inserted_Order_Products (
		order_product_id INT,
		client_product_id INT
	);

	INSERT INTO Order_Products (order_id, product_id, size_id, deal_id, deal_instance_id)
	OUTPUT INSERTED.order_product_id, src.client_product_id INTO #Inserted_Order_Products
	SELECT
		@orderID,
		p.product_id,
		(SELECT s.size_id FROM Sizes as s WHERE s.size_name = p.size_name),
		p.deal_id,
		p.deal_instance_id
	FROM @ProductList AS p;

	INSERT INTO Order_Product_Toppings 
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