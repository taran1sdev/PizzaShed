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
    

