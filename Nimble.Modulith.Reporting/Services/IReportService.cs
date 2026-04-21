namespace Nimble.Modulith.Reporting.Services;

public record OrderReportItem(int OrderId, string Email, DateTime OrderDate, int TotalQuantity, decimal TotalAmount);
public record ProductSalesReportItem(string ProductName, int TotalQuantity, decimal TotalRevenue, int OrderCount);

public record CustomerLifetimeMetrics(decimal TotalSpent, DateTime? FirstOrderDate, DateTime? LastOrderDate, int TotalOrders);
public record CustomerOrderDto(int OrderId, DateTime OrderDate, string ProductName, int Quantity, decimal TotalPrice);
public record CustomerOrdersResponse(CustomerLifetimeMetrics Metrics, IEnumerable<CustomerOrderDto> Orders);

public interface IReportService
{
    Task<IEnumerable<OrderReportItem>> GetOrdersReportAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<ProductSalesReportItem>> GetProductSalesReportAsync(DateTime startDate, DateTime endDate);
    Task<CustomerOrdersResponse> GetCustomerOrdersAsync(int customerId);
}