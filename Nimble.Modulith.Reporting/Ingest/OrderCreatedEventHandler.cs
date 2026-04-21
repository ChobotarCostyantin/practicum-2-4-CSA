using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nimble.Modulith.Customers.Contracts;
using Nimble.Modulith.Reporting.Data;
using Nimble.Modulith.Reporting.Models;

namespace Nimble.Modulith.Reporting.Ingest;

public class OrderCreatedEventHandler(ReportingDbContext dbContext, ILogger<OrderCreatedEventHandler> logger)
    : INotificationHandler<OrderCreatedEvent>
{
    public async ValueTask Handle(OrderCreatedEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Ingesting Order {OrderNumber} into Reporting Database", notification.OrderNumber);

        var dateKey = notification.OrderDate.Year * 10000 + notification.OrderDate.Month * 100 +
                      notification.OrderDate.Day;

        try
        {
            if (!await dbContext.DimCustomers.AnyAsync(c => c.CustomerId == notification.CustomerId, ct))
                dbContext.DimCustomers.Add(new DimCustomer
                    { CustomerId = notification.CustomerId, Email = notification.CustomerEmail, FullName = "Unknown" });

            foreach (var item in notification.Items)
            {
                if (!await dbContext.DimProducts.AnyAsync(p => p.ProductId == item.ProductId, ct))
                    dbContext.DimProducts.Add(new DimProduct
                        { ProductId = item.ProductId, ProductName = item.ProductName });

                var exists =
                    await dbContext.FactOrders.AnyAsync(
                        f => f.OrderId == notification.OrderId && f.OrderItemId == item.Id, ct);
                if (!exists)
                {
                    dbContext.FactOrders.Add(new FactOrder
                    {
                        OrderId = notification.OrderId,
                        OrderItemId = item.Id,
                        DateKey = dateKey,
                        CustomerId = notification.CustomerId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice
                    });
                }
            }

            await dbContext.SaveChangesAsync(ct);
            logger.LogInformation("Successfully ingested order {OrderNumber}", notification.OrderNumber);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to ingest order {OrderNumber}", notification.OrderNumber);
        }
    }
}