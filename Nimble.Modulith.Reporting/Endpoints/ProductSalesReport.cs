using System.Text;
using FastEndpoints;
using Nimble.Modulith.Reporting.Services;

namespace Nimble.Modulith.Reporting.Endpoints;

public class ProductSalesReportRequest
{
    [QueryParam] public DateTime StartDate { get; set; }
    [QueryParam] public DateTime EndDate { get; set; }
    [QueryParam] public string? Format { get; set; }
}

public class ProductSalesReportEndpoint(IReportService reportService) : Endpoint<ProductSalesReportRequest>
{
    public override void Configure()
    {
        Get("/reports/product-sales");
        Roles("Admin");
    }

    public override async Task HandleAsync(ProductSalesReportRequest req, CancellationToken ct)
    {
        var data = await reportService.GetProductSalesReportAsync(req.StartDate, req.EndDate);
        if (req.Format?.ToLower() == "csv" || HttpContext.Request.Headers.Accept.Contains("text/csv"))
        {
            var csv = "ProductName,TotalQuantity,TotalRevenue,OrderCount\n" + string.Join("\n",
                data.Select(d => $"\"{d.ProductName}\",{d.TotalQuantity},{d.TotalRevenue:F2},{d.OrderCount}"));
            await Send.BytesAsync(Encoding.UTF8.GetBytes(csv), "product_sales.csv", "text/csv", cancellation: ct);
            return;
        }

        await Send.OkAsync(data, ct);
    }
}