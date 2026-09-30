using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.Sqlite;

namespace BusinessManagement.App
{
    public class InventoryRepository
    {
        private readonly string connectionString = "Data Source=pos_inventory.db;";

        public InventoryRepository()
        {
        }

        /// <summary>
        /// Creates database tables if they do not already exist.
        /// </summary>
        public void InitializeDatabase()
        {
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();

                string sql = @"
                    CREATE TABLE IF NOT EXISTS Products (
                        ProductID INTEGER PRIMARY KEY AUTOINCREMENT,
                        ProductName TEXT NOT NULL,
                        SKU TEXT NOT NULL UNIQUE,
                        Category TEXT NOT NULL,
                        UnitPrice DECIMAL(10, 2) NOT NULL,
                        StockQuantity INTEGER NOT NULL,
                        ReorderLevel INTEGER NOT NULL DEFAULT 5
                    );

                    CREATE TABLE IF NOT EXISTS Invoices (
                        InvoiceID INTEGER PRIMARY KEY AUTOINCREMENT,
                        CustomerName TEXT NOT NULL,
                        SaleDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                        TotalAmount DECIMAL(10, 2) NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS InvoiceItems (
                        InvoiceItemID INTEGER PRIMARY KEY AUTOINCREMENT,
                        InvoiceID INTEGER NOT NULL,
                        ProductID INTEGER NOT NULL,
                        Quantity INTEGER NOT NULL,
                        UnitPrice DECIMAL(10, 2) NOT NULL,
                        FOREIGN KEY (InvoiceID) REFERENCES Invoices(InvoiceID),
                        FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
                    );";

                using (var cmd = new SqliteCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                // Seed sample products if table is empty
                SeedInitialData(conn);
            }
        }

        private void SeedInitialData(SqliteConnection conn)
        {
            string checkSql = "SELECT COUNT(*) FROM Products;";
            using (var checkCmd = new SqliteCommand(checkSql, conn))
            {
                long count = (long)checkCmd.ExecuteScalar();
                if (count == 0)
                {
                    string seedSql = @"
                        INSERT INTO Products (ProductName, SKU, Category, UnitPrice, StockQuantity, ReorderLevel) VALUES
                        ('Wireless Mouse', 'SKU-001', 'Peripherals', 25.00, 50, 10),
                        ('Mechanical Keyboard', 'SKU-002', 'Peripherals', 85.50, 30, 5),
                        ('27-inch Monitor', 'SKU-003', 'Displays', 220.00, 15, 3),
                        ('USB-C Hub', 'SKU-004', 'Accessories', 45.00, 8, 10);";

                    using (var seedCmd = new SqliteCommand(seedSql, conn))
                    {
                        seedCmd.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves all products from the database into a DataTable.
        /// </summary>
        public DataTable GetAllProducts()
        {
            DataTable dt = new DataTable();

            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT ProductID, ProductName, SKU, Category, UnitPrice, StockQuantity, ReorderLevel FROM Products;";

                using (var cmd = new SqliteCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                }
            }

            return dt;
        }

        /// <summary>
        /// Inserts a new product record into the Products table.
        /// </summary>
        public void AddProduct(string productName, string sku, string category, decimal unitPrice, int stockQuantity, int reorderLevel)
        {
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                string sql = @"
                    INSERT INTO Products (ProductName, SKU, Category, UnitPrice, StockQuantity, ReorderLevel)
                    VALUES (@ProductName, @SKU, @Category, @UnitPrice, @StockQuantity, @ReorderLevel);";

                using (var cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ProductName", productName);
                    cmd.Parameters.AddWithValue("@SKU", sku);
                    cmd.Parameters.AddWithValue("@Category", category);
                    cmd.Parameters.AddWithValue("@UnitPrice", unitPrice);
                    cmd.Parameters.AddWithValue("@StockQuantity", stockQuantity);
                    cmd.Parameters.AddWithValue("@ReorderLevel", reorderLevel);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Saves an invoice transaction and updates stock quantities atomically.
        /// </summary>
        public long SaveInvoice(string customerName, List<CartItem> cart)
        {
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();

                using (var transaction = conn.BeginTransaction())
                {
                    // 1. Insert Invoice Header
                    string insertInvoiceSql = @"
                        INSERT INTO Invoices (CustomerName, TotalAmount)
                        VALUES (@CustomerName, @TotalAmount);
                        SELECT last_insert_rowid();";

                    decimal totalAmount = cart.Sum(item => item.Subtotal);

                    long invoiceId;
                    using (var cmd = new SqliteCommand(insertInvoiceSql, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@CustomerName", customerName);
                        cmd.Parameters.AddWithValue("@TotalAmount", totalAmount);
                        invoiceId = (long)cmd.ExecuteScalar();
                    }

                    // 2. Insert Line Items & Deduct Inventory Stock
                    foreach (var item in cart)
                    {
                        string insertItemSql = @"
                            INSERT INTO InvoiceItems (InvoiceID, ProductID, Quantity, UnitPrice)
                            VALUES (@InvoiceID, @ProductID, @Quantity, @UnitPrice);";

                        using (var cmd = new SqliteCommand(insertItemSql, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@InvoiceID", invoiceId);
                            cmd.Parameters.AddWithValue("@ProductID", item.ProductID);
                            cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                            cmd.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                            cmd.ExecuteNonQuery();
                        }

                        string updateStockSql = @"
                            UPDATE Products
                            SET StockQuantity = StockQuantity - @Quantity
                            WHERE ProductID = @ProductID;";

                        using (var cmd = new SqliteCommand(updateStockSql, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                            cmd.Parameters.AddWithValue("@ProductID", item.ProductID);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                    return invoiceId;
                }
            }
        }
    }
}