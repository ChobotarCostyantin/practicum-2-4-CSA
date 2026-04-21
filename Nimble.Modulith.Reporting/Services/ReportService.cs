using Dapper;
using Microsoft.EntityFrameworkCore;
using Nimble.Modulith.Reporting.Data;

namespace Nimble.Modulith.Reporting.Services;

public class ReportService(ReportingDbContext dbContext) : IReportService
{
    public async Task<IEnumerable<OrderReportItem>> GetOrdersReportAsync(DateTime startDate, DateTime endDate)
    {
        var startKey = startDate.Year * 10000 + startDate.Month * 100 + startDate.Day;
        var endKey = endDate.Year * 10000 + endDate.Month * 100 + endDate.Day;
        var connection = dbContext.Database.GetDbConnection();
        const string sql = """

                                       SELECT f.OrderId, c.Email, d.Date as OrderDate, SUM(f.Quantity) as TotalQuantity, SUM(f.TotalPrice) as TotalAmount
                                       FROM Reporting.FactOrders f
                                       JOIN Reporting.DimCustomer c ON f.CustomerId = c.CustomerId
                                       JOIN Reporting.DimDate d ON f.DateKey = d.DateKey
                                       WHERE f.DateKey BETWEEN @StartKey AND @EndKey
                                       GROUP BY f.OrderId, c.Email, d.Date
                                       ORDER BY d.Date DESC
                           """;
        return await connection.QueryAsync<OrderReportItem>(sql, new { StartKey = startKey, EndKey = endKey });
    }

    public async Task<IEnumerable<ProductSalesReportItem>> GetProductSalesReportAsync(DateTime startDate, DateTime endDate)
    {
        var startKey = startDate.Year * 10000 + startDate.Month * 100 + startDate.Day;
        var endKey = endDate.Year * 10000 + endDate.Month * 100 + endDate.Day;
        var connection = dbContext.Database.GetDbConnection();
        const string sql = """

                                       SELECT p.ProductName, SUM(f.Quantity) as TotalQuantity, SUM(f.TotalPrice) as TotalRevenue, COUNT(DISTINCT f.OrderId) as OrderCount
                                       FROM Reporting.FactOrders f
                                       JOIN Reporting.DimProduct p ON f.ProductId = p.ProductId
                                       WHERE f.DateKey BETWEEN @StartKey AND @EndKey
                                       GROUP BY p.ProductName
                                       ORDER BY TotalRevenue DESC
                           """;
        return await connection.QueryAsync<ProductSalesReportItem>(sql, new { StartKey = startKey, EndKey = endKey });
    }
    
    public async Task<CustomerOrdersResponse> GetCustomerOrdersAsync(int customerId)
    {
        var connection = dbContext.Database.GetDbConnection();
        
        const string metricsSql = """

                                              SELECT 
                                                  COALESCE(SUM(f.TotalPrice), 0) as TotalSpent,
                                                  MIN(d.Date) as FirstOrderDate,
                                                  MAX(d.Date) as LastOrderDate,
                                                  COUNT(DISTINCT f.OrderId) as TotalOrders
                                              FROM Reporting.FactOrders f
                                              JOIN Reporting.DimDate d ON f.DateKey = d.DateKey
                                              WHERE f.CustomerId = @CustomerId
                                  """;
        if (metricsSql == null) throw new ArgumentNullException(nameof(metricsSql));

        var metrics = await connection.QueryFirstOrDefaultAsync<CustomerLifetimeMetrics>(metricsSql, new { CustomerId = customerId });
        
        metrics ??= new CustomerLifetimeMetrics(0, null, null, 0);

        const string ordersSql = """

                                             SELECT 
                                                 f.OrderId, 
                                                 d.Date as OrderDate, 
                                                 p.ProductName, 
                                                 f.Quantity, 
                                                 f.TotalPrice
                                             FROM Reporting.FactOrders f
                                             JOIN Reporting.DimProduct p ON f.ProductId = p.ProductId
                                             JOIN Reporting.DimDate d ON f.DateKey = d.DateKey
                                             WHERE f.CustomerId = @CustomerId
                                             ORDER BY d.Date DESC
                                 """;
            
        var orders = await connection.QueryAsync<CustomerOrderDto>(ordersSql, new { CustomerId = customerId });

        return new CustomerOrdersResponse(metrics, orders);
    }
}