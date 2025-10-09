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
            (SELECT s.size_id FROM Sizes AS s WHERE s.size_name = p.size_name) AS size_id,
            NULLIF(p.deal_id,0) AS deal_id,
            p.deal_instance_id,
            p.client_product_id
        FROM @ProductList AS p
    ) AS Source 
    ON 1 = 0 
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (order_id, product_id, size_id, deal_id, deal_instance_id)
        VALUES (
            @orderID, 
            Source.product_id, 
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
