namespace Nimble.Modulith.Reporting.Models;

public class FactOrder
{
    public int OrderId { get; set; }
    public int OrderItemId { get; set; }
    public int DateKey { get; set; }
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}