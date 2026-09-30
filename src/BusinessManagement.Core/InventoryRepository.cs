using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace BusinessManagement.Core
{
    public class InventoryRepository
    {
        private readonly string _connectionString = "Data Source=inventory.db";

        public InventoryRepository()
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Products (
                        ProductID INTEGER PRIMARY KEY AUTOINCREMENT,
                        SKU TEXT UNIQUE NOT NULL,
                        ProductName TEXT NOT NULL,
                        Category TEXT,
                        UnitPrice DECIMAL(10,2) NOT NULL,
                        StockQuantity INTEGER NOT NULL DEFAULT 0,
                        ReorderLevel INTEGER NOT NULL DEFAULT 5
                    );";

                using (var command = new SqliteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                string countQuery = "SELECT COUNT(*) FROM Products;";
                using (var countCmd = new SqliteCommand(countQuery, connection))
                {
                    long count = (long)countCmd.ExecuteScalar();
                    if (count == 0)
                    {
                        string seedQuery = @"
                            INSERT INTO Products (SKU, ProductName, Category, UnitPrice, StockQuantity, ReorderLevel)
                            VALUES 
                            ('LOGI-MX01', 'Wireless Mouse', 'Peripherals', 25.00, 15, 5),
                            ('KEY-MECH02', 'Mechanical Keyboard', 'Peripherals', 100.00, 8, 3),
                            ('MON-4K27', '27-inch 4K Monitor', 'Displays', 350.00, 2, 5);";

                        using (var seedCmd = new SqliteCommand(seedQuery, connection))
                        {
                            seedCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public List<Product> GetProducts()
        {
            var products = new List<Product>();

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string selectQuery = "SELECT ProductID, SKU, ProductName, Category, UnitPrice, StockQuantity, ReorderLevel FROM Products;";

                using (var command = new SqliteCommand(selectQuery, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            ProductID = reader.GetInt32(0),
                            SKU = reader.GetString(1),
                            ProductName = reader.GetString(2),
                            Category = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            UnitPrice = reader.GetDecimal(4),
                            StockQuantity = reader.GetInt32(5),
                            ReorderLevel = reader.GetInt32(6)
                        });
                    }
                }
            }

            return products;
        }

        public List<Product> SearchProducts(string query)
        {
            var products = new List<Product>();

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string searchSql = @"
                    SELECT ProductID, SKU, ProductName, Category, UnitPrice, StockQuantity, ReorderLevel 
                    FROM Products 
                    WHERE SKU LIKE @query 
                       OR ProductName LIKE @query 
                       OR Category LIKE @query;";

                using (var command = new SqliteCommand(searchSql, connection))
                {
                    command.Parameters.AddWithValue("@query", $"%{query}%");
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(new Product
                            {
                                ProductID = reader.GetInt32(0),
                                SKU = reader.GetString(1),
                                ProductName = reader.GetString(2),
                                Category = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                UnitPrice = reader.GetDecimal(4),
                                StockQuantity = reader.GetInt32(5),
                                ReorderLevel = reader.GetInt32(6)
                            });
                        }
                    }
                }
            }

            return products;
        }

        public void AddProduct(Product product)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string insertSql = @"
                    INSERT INTO Products (SKU, ProductName, Category, UnitPrice, StockQuantity, ReorderLevel)
                    VALUES (@SKU, @ProductName, @Category, @UnitPrice, @StockQuantity, @ReorderLevel);";

                using (var command = new SqliteCommand(insertSql, connection))
                {
                    command.Parameters.AddWithValue("@SKU", product.SKU);
                    command.Parameters.AddWithValue("@ProductName", product.ProductName);
                    command.Parameters.AddWithValue("@Category", product.Category ?? string.Empty);
                    command.Parameters.AddWithValue("@UnitPrice", product.UnitPrice);
                    command.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);
                    command.Parameters.AddWithValue("@ReorderLevel", product.ReorderLevel);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateStock(int productId, int newQuantity)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string updateSql = "UPDATE Products SET StockQuantity = @StockQuantity WHERE ProductID = @ProductID;";

                using (var command = new SqliteCommand(updateSql, connection))
                {
                    command.Parameters.AddWithValue("@StockQuantity", newQuantity);
                    command.Parameters.AddWithValue("@ProductID", productId);

                    command.ExecuteNonQuery();
                }
            }
        }

        public bool DeductStock(int productId, int quantityToDeduct)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                string checkSql = "SELECT StockQuantity FROM Products WHERE ProductID = @ProductID;";
                using (var checkCmd = new SqliteCommand(checkSql, connection))
                {
                    checkCmd.Parameters.AddWithValue("@ProductID", productId);
                    var result = checkCmd.ExecuteScalar();

                    if (result == null) return false;

                    int currentStock = Convert.ToInt32(result);
                    if (currentStock < quantityToDeduct)
                    {
                        return false;
                    }
                }

                string updateSql = "UPDATE Products SET StockQuantity = StockQuantity - @Qty WHERE ProductID = @ProductID;";
                using (var command = new SqliteCommand(updateSql, connection))
                {
                    command.Parameters.AddWithValue("@Qty", quantityToDeduct);
                    command.Parameters.AddWithValue("@ProductID", productId);
                    command.ExecuteNonQuery();
                }
            }
            return true;
        }

        public bool DeductStockBatch(List<CartItem> cartItems)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (var item in cartItems)
                        {
                            string checkSql = "SELECT StockQuantity FROM Products WHERE ProductID = @ProductID;";
                            using (var checkCmd = new SqliteCommand(checkSql, connection, transaction))
                            {
                                checkCmd.Parameters.AddWithValue("@ProductID", item.ProductID);
                                var result = checkCmd.ExecuteScalar();
                                if (result == null || Convert.ToInt32(result) < item.Quantity)
                                {
                                    transaction.Rollback();
                                    return false; // Insufficient stock for this item
                                }
                            }

                            string updateSql = "UPDATE Products SET StockQuantity = StockQuantity - @Qty WHERE ProductID = @ProductID;";
                            using (var updateCmd = new SqliteCommand(updateSql, connection, transaction))
                            {
                                updateCmd.Parameters.AddWithValue("@Qty", item.Quantity);
                                updateCmd.Parameters.AddWithValue("@ProductID", item.ProductID);
                                updateCmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }
    }
}