using BusinessManagement.Core;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace BusinessManagement.App
{
    public class InventoryRepository
    {
        private readonly string connectionString = "Data Source=inventory.db";
        // ADD THIS CONSTRUCTOR:
        public InventoryRepository()
        {
            InitializeDatabase();
        }
        public void InitializeDatabase()
        {
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();

                string tableSuppliers = @"
            CREATE TABLE IF NOT EXISTS Suppliers (
                SupplierID INTEGER PRIMARY KEY AUTOINCREMENT,
                SupplierName TEXT NOT NULL,
                ContactPerson TEXT NOT NULL,
                Phone TEXT NOT NULL,
                Email TEXT NOT NULL
            );";
                using (var cmd = new SqliteCommand(tableSuppliers, conn)) { cmd.ExecuteNonQuery(); }

                // Seed default suppliers if empty
                string checkSuppliers = "SELECT COUNT(*) FROM Suppliers;";
                using (var cmd = new SqliteCommand(checkSuppliers, conn))
                {
                    long count = (long)cmd.ExecuteScalar();
                    if (count == 0)
                    {
                        string seedSuppliers = @"
                    INSERT INTO Suppliers (SupplierName, ContactPerson, Phone, Email) VALUES
                    ('Global Peripherals Ltd.', 'Marc Dupont', '+230 5712 3456', 'orders@globalperipherals.mu'),
                    ('TechCorp Logistics', 'Aisha Beeharry', '+230 5988 7890', 'supply@techcorp.mu'),
                    ('Island Office Solutions', 'Kevin Naidu', '+230 5433 1122', 'sales@islandoffice.mu');
                ";
                        using (var seedCmd = new SqliteCommand(seedSuppliers, conn)) { seedCmd.ExecuteNonQuery(); }
                    }
                }

                // Customers Table
                string tableCustomers = @"
    CREATE TABLE IF NOT EXISTS Customers (
        CustomerID INTEGER PRIMARY KEY AUTOINCREMENT,
        FidelityCardCode TEXT UNIQUE,
        CustomerName TEXT,
        Phone TEXT,
        Email TEXT,
        PointsBalance INTEGER DEFAULT 0,
        StoreCredit DECIMAL(18,2) DEFAULT 0,
        LifetimeSpend DECIMAL(18,2) DEFAULT 0
    );";
                using (var cmd = new SqliteCommand(tableCustomers, conn)) { cmd.ExecuteNonQuery(); }

                // Seed default fidelity customers if empty
                string checkCustomers = "SELECT COUNT(*) FROM Customers;";
                using (var cmd = new SqliteCommand(checkCustomers, conn))
                {
                    long count = (long)cmd.ExecuteScalar();
                    if (count == 0)
                    {
                        string seedCustomers = @"
            INSERT INTO Customers (FidelityCardCode, CustomerName, Phone, Email, PointsBalance, StoreCredit, LifetimeSpend) VALUES
            ('FID-1001', 'Jean-Luc Dubois', '+230 5712 3456', 'jeanluc.d@outlook.com', 450, 1250.00, 14200.00),
            ('FID-1002', 'Aisha Ramchurn', '+230 5988 9012', 'aisha.ram@gmail.com', 820, 0.00, 28900.50),
            ('FID-1003', 'Kunal Beeharry', '+230 5433 1122', 'kunal.b@mauritius.mu', 150, 3400.50, 9850.00);
        ";
                        using (var seedCmd = new SqliteCommand(seedCustomers, conn)) { seedCmd.ExecuteNonQuery(); }
                    }
                }

                // Added CHECK constraint: StockQuantity CANNOT drop below 0 at the database level
                string tableProducts = @"
            CREATE TABLE IF NOT EXISTS Products (
                ProductID INTEGER PRIMARY KEY AUTOINCREMENT,
                ProductName TEXT NOT NULL,
                SKU TEXT UNIQUE NOT NULL,
                Category TEXT NOT NULL,
                UnitPrice REAL NOT NULL,
                StockQuantity INTEGER NOT NULL CHECK (StockQuantity >= 0),
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

                // Automatically fix any legacy negative stocks on startup
                string fixNegativeQuery = "UPDATE Products SET StockQuantity = ReorderLevel * 2 WHERE StockQuantity < 0;";
                using (var fixCmd = new SqliteCommand(fixNegativeQuery, conn))
                {
                    fixCmd.ExecuteNonQuery();
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

                            string updateStock = "UPDATE Products SET StockQuantity = StockQuantity - @Quantity WHERE ProductID = @ProductID AND StockQuantity >= @Quantity;";
                            using (var cmd = new SqliteCommand(updateStock, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                                cmd.Parameters.AddWithValue("@ProductID", item.ProductID);
                                int rowsAffected = cmd.ExecuteNonQuery();
                                // If no rows were affected, it means stock is insufficient; abort transaction!
                                if (rowsAffected == 0)
                                {
                                    throw new InvalidOperationException($"Insufficient stock for product ID {item.ProductID}. Transaction aborted.");
                                }
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
        public DataTable GetAllSuppliers()
        {
            DataTable dt = new DataTable();
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT SupplierID, SupplierName, ContactPerson, Phone, Email FROM Suppliers;";
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

        public void PlaceRestockOrder(long supplierId, string supplierName, int productId, int quantity, decimal totalCost)
        {
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Log as an invoice/purchase record
                        string insertInvoice = "INSERT INTO Invoices (CustomerName, SaleDate, TotalAmount) VALUES (@CustomerName, @SaleDate, @TotalAmount); SELECT last_insert_rowid();";
                        long invoiceId;
                        using (var cmd = new SqliteCommand(insertInvoice, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@CustomerName", $"Restock: {supplierName}");
                            cmd.Parameters.AddWithValue("@SaleDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.Parameters.AddWithValue("@TotalAmount", totalCost);
                            invoiceId = (long)cmd.ExecuteScalar();
                        }

                        // 2. Increment stock quantity in products
                        string updateStock = "UPDATE Products SET StockQuantity = StockQuantity + @Quantity WHERE ProductID = @ProductID;";
                        using (var cmd = new SqliteCommand(updateStock, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Quantity", quantity);
                            cmd.Parameters.AddWithValue("@ProductID", productId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        public int GetLowStockCount()
        {
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM Products WHERE StockQuantity <= ReorderLevel;";
                using (var cmd = new SqliteCommand(query, conn))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }
        public DataTable GetAutomatedRestockQueue()
        {
            DataTable dt = new DataTable();
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                string query = @"
            SELECT 
                p.ProductID,
                p.ProductName,
                p.SKU,
                p.StockQuantity,
                p.ReorderLevel,
                (p.ReorderLevel * 2 - p.StockQuantity) AS RecommendedQty,
                COALESCE(s.SupplierID, 1) AS SupplierID,
                COALESCE(s.SupplierName, 'Global Peripherals Ltd.') AS SupplierName,
                ((p.ReorderLevel * 2 - p.StockQuantity) * p.UnitPrice) AS EstimatedCost
            FROM Products p
            LEFT JOIN Suppliers s ON s.SupplierID = 1
            WHERE p.StockQuantity <= p.ReorderLevel;";

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
        public void ResetNegativeStocks()
        {
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE Products SET StockQuantity = ReorderLevel * 2 WHERE StockQuantity < 0;";
                using (var cmd = new SqliteCommand(query, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public void SeedDefaultData()
        {
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();

                // Check if products already exist
                string checkQuery = "SELECT COUNT(*) FROM Products;";
                using (var checkCmd = new SqliteCommand(checkQuery, conn))
                {
                    long count = (long)checkCmd.ExecuteScalar();
                    if (count > 0) return; // Already seeded
                }

                // Insert default suppliers first
                string insertSuppliers = @"
            INSERT INTO Suppliers (SupplierName, ContactPerson, Email, Phone) 
            VALUES ('Global Peripherals Ltd.', 'John Smith', 'contact@globalperipherals.com', '555-0192');";
                using (var cmd = new SqliteCommand(insertSuppliers, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                // Insert healthy, positive starter products with correct reorder levels
                string insertProducts = @"
            INSERT INTO Products (ProductName, SKU, Category, UnitPrice, StockQuantity, ReorderLevel) VALUES 
            ('Wireless Mouse', 'LOGI-MX01', 'Peripherals', 25.00, 25, 5),
            ('Mechanical Keyboard', 'KEY-MECH02', 'Peripherals', 100.00, 15, 3),
            ('27-inch 4K Monitor', 'MON-4K27', 'Displays', 350.00, 10, 5),
            ('Studio Earphones', 'EAR-2KLP', 'Peripherals', 20.00, 40, 10);";
                using (var cmd = new SqliteCommand(insertProducts, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public (decimal grossRevenue, int transactionCount, int lowStockCount) GetLiveShiftMetrics()
        {
            decimal revenue = 0;
            int txCount = 0;
            int lowStock = 0;

            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();

                // 1. Get today's total revenue and transaction count
                string salesQuery = @"
            SELECT COALESCE(SUM(TotalAmount), 0), COUNT(InvoiceID) 
            FROM Invoices 
            WHERE DATE(SaleDate) = DATE('now');";

                using (var cmd = new SqliteCommand(salesQuery, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            revenue = reader.GetDecimal(0);
                            txCount = reader.GetInt32(1);
                        }
                    }
                }

                // 2. Get current low stock count
                string lowStockQuery = "SELECT COUNT(*) FROM Products WHERE StockQuantity <= ReorderLevel;";
                using (var cmd = new SqliteCommand(lowStockQuery, conn))
                {
                    lowStock = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }

            return (revenue, txCount, lowStock);
        }
        public DataTable GetRecentSalesTrend()
        {
            DataTable dt = new DataTable();
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                string query = @"
            SELECT 
                DATE(SaleDate) AS SaleDate,
                SUM(TotalAmount) AS DailyRevenue
            FROM Invoices
            GROUP BY DATE(SaleDate)
            ORDER BY SaleDate ASC
            LIMIT 7;";

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
        public DataTable GetTopSellingProducts()
        {
            DataTable dt = new DataTable();
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT 
                        p.ProductName AS ItemName,
                        SUM(ii.Quantity) AS TotalSold,
                        SUM(ii.Quantity * ii.UnitPrice) AS Revenue
                    FROM InvoiceItems ii
                    JOIN Invoices i ON ii.InvoiceID = i.InvoiceID
                    JOIN Products p ON ii.ProductID = p.ProductID
                    WHERE DATE(i.SaleDate) = DATE('now')
                    GROUP BY p.ProductID, p.ProductName
                    ORDER BY TotalSold DESC
                    LIMIT 5;";

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
        public DataTable GetCriticalLowStockItems()
        {
            DataTable dt = new DataTable();
            using (var conn = new Microsoft.Data.Sqlite.SqliteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT Id, Name, StockQuantity, ReorderLevel FROM Inventory WHERE StockQuantity <= ReorderLevel";
                using (var cmd = new Microsoft.Data.Sqlite.SqliteCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            return dt;
        }
        public CustomerFidelityModel GetCustomerByFidelityCard(string cardCode)
        {
            CustomerFidelityModel customer = null;
            string query = @"SELECT CustomerID, FidelityCardCode, CustomerName, Phone, PointsBalance, StoreCredit 
                     FROM Customers 
                     WHERE FidelityCardCode = @CardCode";

            using (SqliteConnection conn = new SqliteConnection(connectionString))
            {
                using (SqliteCommand cmd = new SqliteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CardCode", cardCode.Trim());
                    conn.Open();
                    using (SqliteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            customer = new CustomerFidelityModel
                            {
                                CustomerID = reader.GetInt32(0),
                                FidelityCardCode = reader.GetString(1),
                                CustomerName = reader.GetString(2),
                                Phone = reader.GetString(3),
                                PointsBalance = reader.GetInt32(4),
                                StoreCredit = reader.GetDecimal(5)
                            };
                        }
                    }
                }
            }
            return customer;
        }
        public void UpdateCustomerLoyalty(int customerID, int newPointsBalance, decimal newStoreCredit)
        {
            string query = @"UPDATE Customers 
                     SET PointsBalance = @Points, StoreCredit = @Credit 
                     WHERE CustomerID = @ID";

            using (SqliteConnection conn = new SqliteConnection(connectionString))
            {
                using (SqliteCommand cmd = new SqliteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Points", newPointsBalance);
                    cmd.Parameters.AddWithValue("@Credit", newStoreCredit);
                    cmd.Parameters.AddWithValue("@ID", customerID);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
        // 1. Fetch all customers dynamically for the DataGridView
        public List<CustomerModel> GetAllCustomers()
        {
            var customers = new List<CustomerModel>();
            string query = "SELECT CustomerID, FidelityCardCode, CustomerName, Phone, Email, PointsBalance, StoreCredit, LifetimeSpend FROM Customers;";

            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            customers.Add(new CustomerModel
                            {
                                CustomerID = reader.GetInt32(0),
                                FidelityCardCode = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                CustomerName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                Phone = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                                Email = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                                PointsBalance = reader.GetInt32(5),
                                StoreCredit = reader.GetDecimal(6),
                                LifetimeSpend = reader.GetDecimal(7)
                            });
                        }
                    }
                }
            }
            return customers;
        }

        // 2. Add a new customer
        public void AddCustomer(CustomerModel customer)
        {
            string query = @"INSERT INTO Customers (FidelityCardCode, CustomerName, Phone, Email, PointsBalance, StoreCredit, LifetimeSpend) 
                     VALUES (@Code, @Name, @Phone, @Email, @Points, @Credit, @Spend);";

            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Code", customer.FidelityCardCode ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Name", customer.CustomerName ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Phone", customer.Phone ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Email", customer.Email ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Points", customer.PointsBalance);
                    cmd.Parameters.AddWithValue("@Credit", customer.StoreCredit);
                    cmd.Parameters.AddWithValue("@Spend", customer.LifetimeSpend);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // 3. Update existing customer info (like phone number or name)
        public void UpdateCustomer(CustomerModel customer)
        {
            string query = @"UPDATE Customers SET CustomerName = @Name, Phone = @Phone, Email = @Email, 
                     PointsBalance = @Points, StoreCredit = @Credit WHERE CustomerID = @ID;";

            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", customer.CustomerID);
                    cmd.Parameters.AddWithValue("@Name", customer.CustomerName ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Phone", customer.Phone ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Email", customer.Email ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Points", customer.PointsBalance);
                    cmd.Parameters.AddWithValue("@Credit", customer.StoreCredit);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // 4. Delete a customer record
        public void DeleteCustomer(int customerId)
        {
            string query = "DELETE FROM Customers WHERE CustomerID = @ID;";

            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", customerId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public List<HourlySalesModel> GetHourlySalesToday()
        {
            var hourlySales = new List<HourlySalesModel>();

            // Example query using SQLite / ADO.NET matching your project setup:
            string query = @"
        SELECT 
            CAST(strftime('%H', SaleTimestamp) AS INTEGER) as SaleHour, 
            SUM(TotalAmount) as HourlyRevenue 
        FROM Invoices 
        WHERE date(SaleTimestamp) = date('now') 
        GROUP BY SaleHour";

            // Execute your command/connection here and populate hourlySales...

            return hourlySales;
        }
    }
}