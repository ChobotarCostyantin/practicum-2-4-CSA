using System.Text;
using FastEndpoints;
using Nimble.Modulith.Reporting.Services;

namespace Nimble.Modulith.Reporting.Endpoints;

public class CustomerOrdersReportRequest
{
    public int CustomerId { get; set; } 
    [QueryParam] public string? Format { get; set; }
}

public class CustomerOrdersReportEndpoint(IReportService reportService) : Endpoint<CustomerOrdersReportRequest, CustomerOrdersResponse>
{
    public override void Configure()
    {
        Get("/reports/customers/{customerId}/orders");
        Roles("Admin"); 
    }

    public override async Task HandleAsync(CustomerOrdersReportRequest req, CancellationToken ct)
    {
        var data = await reportService.GetCustomerOrdersAsync(req.CustomerId);

        if (req.Format?.ToLower() == "csv" || HttpContext.Request.Headers.Accept.Contains("text/csv"))
        {
            var csv = "OrderId,OrderDate,ProductName,Quantity,TotalPrice\n" + string.Join("\n",
                data.Orders.Select(d => $"{d.OrderId},{d.OrderDate:yyyy-MM-dd},\"{d.ProductName}\",{d.Quantity},{d.TotalPrice:F2}"));
            
            await Send.BytesAsync(Encoding.UTF8.GetBytes(csv), $"customer_{req.CustomerId}_orders.csv", "text/csv", cancellation: ct);
            return;
        }

        await Send.OkAsync(data, cancellation: ct);
    }
}