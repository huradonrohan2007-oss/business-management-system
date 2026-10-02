namespace BusinessManagement.Core
{
    public class CustomerFidelityModel
    {
        public int CustomerID { get; set; }
        public string FidelityCardCode { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public int PointsBalance { get; set; }
        public decimal StoreCredit { get; set; }
    }
}