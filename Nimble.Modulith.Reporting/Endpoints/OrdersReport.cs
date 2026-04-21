using System.Text;
using FastEndpoints;
using Nimble.Modulith.Reporting.Services;

namespace Nimble.Modulith.Reporting.Endpoints;

public class OrdersReportRequest
{
    [QueryParam] public DateTime StartDate { get; set; }
    [QueryParam] public DateTime EndDate { get; set; }
    [QueryParam] public string? Format { get; set; }
}

public class OrdersReportEndpoint(IReportService reportService) : Endpoint<OrdersReportRequest>
{
    public override void Configure()
    {
        Get("/reports/orders");
        Roles("Admin");
    }

    public override async Task HandleAsync(OrdersReportRequest req, CancellationToken ct)
    {
        var data = await reportService.GetOrdersReportAsync(req.StartDate, req.EndDate);
        if (req.Format?.ToLower() == "csv" || HttpContext.Request.Headers.Accept.Contains("text/csv"))
        {
            var csv = "OrderId,Email,OrderDate,TotalQuantity,TotalAmount\n" + string.Join("\n",
                data.Select(d =>
                    $"{d.OrderId},{d.Email},{d.OrderDate:yyyy-MM-dd},{d.TotalQuantity},{d.TotalAmount:F2}"));
            await Send.BytesAsync(Encoding.UTF8.GetBytes(csv), "orders_report.csv", "text/csv", cancellation: ct);
            return;
        }

        await Send.OkAsync(data, ct);
    }
}