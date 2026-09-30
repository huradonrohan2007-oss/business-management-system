namespace BusinessManagement.App
{
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        // Calculated line item subtotal
        public decimal Subtotal => UnitPrice * Quantity;
    }
}