using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;

namespace BusinessManagement.App
{
    public class InventoryRepository
    {
        private readonly string connectionString = "Data Source=inventory.db";

        public void InitializeDatabase()
        {
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();

                string tableProducts = @"
                    CREATE TABLE IF NOT EXISTS Products (
                        ProductID INTEGER PRIMARY KEY AUTOINCREMENT,
                        ProductName TEXT NOT NULL,
                        SKU TEXT UNIQUE NOT NULL,
                        Category TEXT NOT NULL,
                        UnitPrice REAL NOT NULL,
                        StockQuantity INTEGER NOT NULL,
                        ReorderLevel INTEGER NOT NULL DEFAULT 10
                    );";

                string tableInvoices = @"
                    CREATE TABLE IF NOT EXISTS Invoices (
                        InvoiceID INTEGER PRIMARY KEY AUTOINCREMENT,
                        CustomerName TEXT NOT NULL,
                        SaleDate DATETIME NOT NULL,
                        TotalAmount REAL NOT NULL
                    );";

                string tableInvoiceItems = @"
                    CREATE TABLE IF NOT EXISTS InvoiceItems (
                        ItemID INTEGER PRIMARY KEY AUTOINCREMENT,
                        InvoiceID INTEGER NOT NULL,
                        ProductID INTEGER NOT NULL,
                        Quantity INTEGER NOT NULL,
                        UnitPrice REAL NOT NULL,
                        FOREIGN KEY (InvoiceID) REFERENCES Invoices(InvoiceID),
                        FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
                    );";

                using (var cmd = new SqliteCommand(tableProducts, conn)) { cmd.ExecuteNonQuery(); }
                using (var cmd = new SqliteCommand(tableInvoices, conn)) { cmd.ExecuteNonQuery(); }
                using (var cmd = new SqliteCommand(tableInvoiceItems, conn)) { cmd.ExecuteNonQuery(); }

                // Seed sample products if table is empty
                string checkCount = "SELECT COUNT(*) FROM Products;";
                using (var cmd = new SqliteCommand(checkCount, conn))
                {
                    long count = (long)cmd.ExecuteScalar();
                    if (count == 0)
                    {
                        string seed = @"
                            INSERT INTO Products (ProductName, SKU, Category, UnitPrice, StockQuantity, ReorderLevel) VALUES
                            ('Wireless Mouse', 'SKU-001', 'Peripherals', 25.00, 45, 10),
                            ('Mechanical Keyboard', 'SKU-002', 'Peripherals', 75.00, 8, 15),
                            ('27 inch Monitor', 'SKU-003', 'Displays', 299.99, 5, 10),
                            ('USB-C Hub', 'SKU-004', 'Accessories', 35.50, 22, 10);
                        ";
                        using (var seedCmd = new SqliteCommand(seed, conn)) { seedCmd.ExecuteNonQuery(); }
                    }
                }
            }
        }

        public DataTable GetAllProducts()
        {
            DataTable dt = new DataTable();
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT ProductID, ProductName, SKU, Category, UnitPrice, StockQuantity, ReorderLevel FROM Products;";
                using (var cmd = new SqliteCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            return dt;
        }

        public DataTable GetLowStockProducts()
        {
            DataTable dt = new DataTable();
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT ProductID, ProductName, SKU, UnitPrice, StockQuantity, ReorderLevel FROM Products WHERE StockQuantity <= ReorderLevel;";
                using (var cmd = new SqliteCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            return dt;
        }

        public int CalculateRestockQuantity(int currentStock, int reorderLevel)
        {
            int target = Math.Max(reorderLevel * 2, 20);
            int needed = target - currentStock;
            return needed > 0 ? needed : 10;
        }

        public void UpdateReorderLevel(int productId, int newReorderLevel)
        {
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE Products SET ReorderLevel = @ReorderLevel WHERE ProductID = @ProductID;";
                using (var cmd = new SqliteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ReorderLevel", newReorderLevel);
                    cmd.Parameters.AddWithValue("@ProductID", productId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public long SaveInvoice(string customerName, List<CartItem> cartItems)
        {
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        decimal totalAmount = 0;
                        foreach (var item in cartItems)
                        {
                            totalAmount += item.Subtotal;
                        }

                        string insertInvoice = "INSERT INTO Invoices (CustomerName, SaleDate, TotalAmount) VALUES (@CustomerName, @SaleDate, @TotalAmount); SELECT last_insert_rowid();";
                        long invoiceId;
                        using (var cmd = new SqliteCommand(insertInvoice, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@CustomerName", customerName);
                            cmd.Parameters.AddWithValue("@SaleDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.Parameters.AddWithValue("@TotalAmount", totalAmount);
                            invoiceId = (long)cmd.ExecuteScalar();
                        }

                        foreach (var item in cartItems)
                        {
                            string insertItem = "INSERT INTO InvoiceItems (InvoiceID, ProductID, Quantity, UnitPrice) VALUES (@InvoiceID, @ProductID, @Quantity, @UnitPrice);";
                            using (var cmd = new SqliteCommand(insertItem, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@InvoiceID", invoiceId);
                                cmd.Parameters.AddWithValue("@ProductID", item.ProductID);
                                cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                                cmd.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                                cmd.ExecuteNonQuery();
                            }

                            string updateStock = "UPDATE Products SET StockQuantity = StockQuantity - @Quantity WHERE ProductID = @ProductID;";
                            using (var cmd = new SqliteCommand(updateStock, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                                cmd.Parameters.AddWithValue("@ProductID", item.ProductID);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return invoiceId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public DataTable GetAllInvoices()
        {
            DataTable dt = new DataTable();
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT InvoiceID, CustomerName, SaleDate, TotalAmount FROM Invoices ORDER BY SaleDate DESC;";
                using (var cmd = new SqliteCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            return dt;
        }

        public DataTable GetInvoiceItems(long invoiceId)
        {
            DataTable dt = new DataTable();
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT 
                        p.ProductName,
                        ii.Quantity,
                        ii.UnitPrice,
                        (ii.Quantity * ii.UnitPrice) AS Subtotal
                    FROM InvoiceItems ii
                    JOIN Products p ON ii.ProductID = p.ProductID
                    WHERE ii.InvoiceID = @InvoiceID;";

                using (var cmd = new SqliteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@InvoiceID", invoiceId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            return dt;
        }

        public DataTable GetDailySalesSummary(DateTime targetDate)
        {
            DataTable dt = new DataTable();
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT 
                        STRFTIME('%Y-%m-%d', SaleDate) AS SaleDay,
                        COUNT(InvoiceID) AS TotalTransactions,
                        SUM(TotalAmount) AS GrossRevenue
                    FROM Invoices
                    WHERE DATE(SaleDate) = DATE(@TargetDate)
                    GROUP BY SaleDay;";

                using (var cmd = new SqliteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TargetDate", targetDate.ToString("yyyy-MM-dd"));
                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            return dt;
        }

        public DataTable GetItemizedSalesSummary(DateTime targetDate)
        {
            DataTable dt = new DataTable();
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT 
                        p.ProductName,
                        SUM(ii.Quantity) AS TotalUnitsSold,
                        SUM(ii.Quantity * ii.UnitPrice) AS TotalSalesRevenue
                    FROM InvoiceItems ii
                    JOIN Invoices i ON ii.InvoiceID = i.InvoiceID
                    JOIN Products p ON ii.ProductID = p.ProductID
                    WHERE DATE(i.SaleDate) = DATE(@TargetDate)
                    GROUP BY p.ProductID, p.ProductName
                    ORDER BY TotalUnitsSold DESC;";

                using (var cmd = new SqliteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TargetDate", targetDate.ToString("yyyy-MM-dd"));
                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            return dt;
        }
    }
}