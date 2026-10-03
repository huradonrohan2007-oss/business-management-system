-- Inventory / Products Table
CREATE TABLE Products (
    ProductID INT PRIMARY KEY IDENTITY(1,1),
    SKU VARCHAR(50) UNIQUE NOT NULL,
    ProductName VARCHAR(100) NOT NULL,
    Category VARCHAR(50),
    UnitPrice DECIMAL(10,2) NOT NULL,
    StockQuantity INT NOT NULL DEFAULT 0,
    ReorderLevel INT NOT NULL DEFAULT 5
);

-- Invoices / Transactions Table
CREATE TABLE Invoices (
    InvoiceID INT PRIMARY KEY IDENTITY(1001,1),
    CustomerName VARCHAR(100) NOT NULL,
    InvoiceDate DATETIME DEFAULT GETDATE(),
    TotalAmount DECIMAL(10,2) NOT NULL,
    PaymentStatus VARCHAR(20) DEFAULT 'Paid'
);

-- Invoice Line Items Table
CREATE TABLE InvoiceItems (
    ItemID INT PRIMARY KEY IDENTITY(1,1),
    InvoiceID INT FOREIGN KEY REFERENCES Invoices(InvoiceID),
    ProductID INT FOREIGN KEY REFERENCES Products(ProductID),
    Quantity INT NOT NULL,
    LineTotal DECIMAL(10,2) NOT NULL
);

SELECT 
    DATEPART(hour, InvoiceDate) AS SaleHour,
    SUM(TotalAmount) AS HourlyRevenue,
    COUNT(InvoiceID) AS TransactionCount
FROM Invoices
WHERE CAST(InvoiceDate AS DATE) = CAST(GETDATE() AS DATE)
GROUP BY DATEPART(hour, InvoiceDate)
ORDER BY SaleHour;
