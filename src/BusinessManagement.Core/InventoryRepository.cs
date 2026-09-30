using System.Collections.Generic;

namespace BusinessManagement.Core
{
    public class InventoryRepository
    {
        public List<Product> GetProducts()
        {
            return new List<Product>
            {
                new Product { ProductID = 1, SKU = "LOGI-MX01", ProductName = "Wireless Mouse", Category = "Peripherals", UnitPrice = 25.00m, StockQuantity = 15, ReorderLevel = 5 },
                new Product { ProductID = 2, SKU = "KEY-MECH02", ProductName = "Mechanical Keyboard", Category = "Peripherals", UnitPrice = 100.00m, StockQuantity = 8, ReorderLevel = 3 },
                new Product { ProductID = 3, SKU = "MON-4K27", ProductName = "27-inch 4K Monitor", Category = "Displays", UnitPrice = 350.00m, StockQuantity = 2, ReorderLevel = 5 }
            };
        }
    }
}