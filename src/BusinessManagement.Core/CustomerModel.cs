using System;

public class CustomerModel
{
    public int CustomerID { get; set; }
    public string FidelityCardCode { get; set; }
    public string CustomerName { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public int PointsBalance { get; set; }
    public decimal StoreCredit { get; set; }
    public decimal LifetimeSpend { get; set; }
}