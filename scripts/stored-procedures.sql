-- ============================================
-- InvenTrack Enterprise - Stored Procedures
-- ============================================

-- 1. sp_GetInventorySummary
CREATE OR ALTER PROCEDURE sp_GetInventorySummary
AS
BEGIN
    SELECT
        COUNT(*) AS TotalProducts,
        ISNULL(SUM(p.CostPrice * p.StockQuantity), 0) AS TotalStockValue,
        COUNT(CASE WHEN p.StockQuantity > 0 AND p.StockQuantity <= p.MinStockThreshold THEN 1 END) AS LowStockCount,
        COUNT(CASE WHEN p.StockQuantity <= 0 THEN 1 END) AS OutOfStockCount
    FROM Products p
    WHERE p.IsActive = 1;
END;
GO

-- 2. sp_GetLowStockProducts
CREATE OR ALTER PROCEDURE sp_GetLowStockProducts
AS
BEGIN
    SELECT
        p.Id AS ProductId,
        p.SKU,
        p.Name AS ProductName,
        c.Name AS CategoryName,
        s.Name AS SupplierName,
        s.Email AS SupplierEmail,
        s.Phone AS SupplierPhone,
        p.StockQuantity,
        p.MinStockThreshold,
        p.UnitPrice
    FROM Products p
    LEFT JOIN Categories c ON p.CategoryId = c.Id
    LEFT JOIN Suppliers s ON p.SupplierId = s.Id
    WHERE p.IsActive = 1 AND p.StockQuantity <= p.MinStockThreshold
    ORDER BY p.StockQuantity ASC;
END;
GO

-- 3. sp_GetStockMovementReport
CREATE OR ALTER PROCEDURE sp_GetStockMovementReport
    @FromDate DATETIME = NULL,
    @ToDate DATETIME = NULL,
    @ProductId INT = NULL,
    @Type NVARCHAR(20) = NULL
AS
BEGIN
    SELECT
        sm.Id AS MovementId,
        p.Name AS ProductName,
        p.SKU,
        sm.MovementType,
        sm.Quantity,
        sm.QuantityBefore,
        sm.QuantityAfter,
        sm.Reference,
        u.Username AS UserName,
        sm.CreatedAt
    FROM StockMovements sm
    INNER JOIN Products p ON sm.ProductId = p.Id
    INNER JOIN Users u ON sm.UserId = u.Id
    WHERE (@FromDate IS NULL OR sm.CreatedAt >= @FromDate)
      AND (@ToDate IS NULL OR sm.CreatedAt <= @ToDate)
      AND (@ProductId IS NULL OR sm.ProductId = @ProductId)
      AND (@Type IS NULL OR sm.MovementType = @Type)
    ORDER BY sm.CreatedAt DESC;
END;
GO

-- 4. sp_GetTopMovingProducts
CREATE OR ALTER PROCEDURE sp_GetTopMovingProducts
    @FromDate DATETIME = NULL,
    @ToDate DATETIME = NULL,
    @Top INT = 10
AS
BEGIN
    SELECT TOP (@Top)
        p.Id AS ProductId,
        p.Name AS ProductName,
        p.SKU,
        c.Name AS CategoryName,
        COUNT(*) AS TotalMovements,
        SUM(sm.Quantity) AS TotalQuantity
    FROM StockMovements sm
    INNER JOIN Products p ON sm.ProductId = p.Id
    LEFT JOIN Categories c ON p.CategoryId = c.Id
    WHERE (@FromDate IS NULL OR sm.CreatedAt >= @FromDate)
      AND (@ToDate IS NULL OR sm.CreatedAt <= @ToDate)
    GROUP BY p.Id, p.Name, p.SKU, c.Name
    ORDER BY TotalMovements DESC;
END;
GO

-- 5. sp_GetInventoryValuation
CREATE OR ALTER PROCEDURE sp_GetInventoryValuation
AS
BEGIN
    SELECT
        c.Name AS CategoryName,
        COUNT(p.Id) AS ProductCount,
        ISNULL(SUM(p.StockQuantity), 0) AS TotalStock,
        ISNULL(SUM(p.CostPrice * p.StockQuantity), 0) AS TotalStockValue
    FROM Categories c
    LEFT JOIN Products p ON c.Id = p.CategoryId AND p.IsActive = 1
    GROUP BY c.Id, c.Name
    ORDER BY TotalStockValue DESC;
END;
GO

-- 6. sp_UpdateStockQuantity (transaction-safe atomic stock update)
CREATE OR ALTER PROCEDURE sp_UpdateStockQuantity
    @ProductId INT,
    @QuantityChange INT,
    @MovementType NVARCHAR(20),
    @UserId INT,
    @Reference NVARCHAR(200) = NULL,
    @Notes NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        DECLARE @CurrentQty INT;
        DECLARE @NewQty INT;

        SELECT @CurrentQty = StockQuantity
        FROM Products WITH (UPDLOCK, ROWLOCK)
        WHERE Id = @ProductId;

        IF @CurrentQty IS NULL
            THROW 50001, 'Product not found', 1;

        SET @NewQty = @CurrentQty + @QuantityChange;

        IF @NewQty < 0
            THROW 50002, 'Insufficient stock', 1;

        UPDATE Products
        SET StockQuantity = @NewQty, UpdatedAt = GETUTCDATE()
        WHERE Id = @ProductId;

        INSERT INTO StockMovements (ProductId, MovementType, Quantity, QuantityBefore, QuantityAfter, Reference, Notes, UserId, CreatedAt)
        VALUES (@ProductId, @MovementType, ABS(@QuantityChange), @CurrentQty, @NewQty, @Reference, @Notes, @UserId, GETUTCDATE());

        COMMIT TRANSACTION;

        SELECT @NewQty AS NewStockQuantity;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO
