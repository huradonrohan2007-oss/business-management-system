namespace BusinessManagement.Core
{
    public class CartItem
    {
        public int ProductID { get; set; }
        public string SKU { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        // Calculated property for line item total
        public decimal Subtotal => UnitPrice * Quantity;
    }
}