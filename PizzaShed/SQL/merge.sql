IF OBJECT_ID('dbo.CreateCollectionOrder', 'P') IS NOT NULL
    DROP PROCEDURE CreateCollectionOrder
GO

CREATE PROCEDURE CreateCollectionOrder
    @userID INT,
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

    -- 1. Insert into Orders
    INSERT INTO Orders(user_id, order_status_id, order_date, order_type, paid, total_price)    
    VALUES (
        @userID,
        @statusID,
        GETDATE(),
        'Collection',
        0,
        @price);

    SET @orderID = SCOPE_IDENTITY();

    -- Check if Order insert failed
    IF @orderID IS NULL OR @orderID = 0
    BEGIN
        ROLLBACK TRANSACTION;
        SELECT 0 AS NewOrderID;
        RETURN;
    END

    -- 2. Create temp table for output mapping
    CREATE TABLE #Inserted_Order_Products (
        order_product_id INT,
        client_product_id INT  -- Note: Match case to TVP (client_product_id)
    );

    -- 3. Use MERGE to insert products and capture both IDs efficiently
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
    ON 1 = 0 -- Always False condition ensures only INSERT operations occur
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (order_id, product_id, size_id, deal_id, deal_instance_id)
        VALUES (
            @orderID, 
            Source.product_id, 
            Source.size_id, 
            Source.deal_id, 
            Source.deal_instance_id
        )
    -- This OUTPUT clause can access both the INSERTED table and the Source table (Source.client_product_id)
    OUTPUT INSERTED.order_product_id, Source.client_product_id INTO #Inserted_Order_Products;

    -- 4. Insert Toppings
    INSERT INTO Order_Product_Toppings (order_product_id, topping_id) 
    SELECT 
        iop.order_product_id,
        t.topping_id
    FROM #Inserted_Order_Products AS iop
    INNER JOIN @ToppingList AS t ON iop.client_product_id = t.client_product_id;

    -- 5. Clean up temp table
    DROP TABLE #Inserted_Order_Products;

    -- 6. Final check and commit
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
