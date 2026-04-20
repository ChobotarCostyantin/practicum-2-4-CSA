using FastEndpoints;
using Mediator;
using Nimble.Modulith.Customers.UseCases.Orders.Commands;

namespace Nimble.Modulith.Customers.Endpoints.Orders;

public class AddItem(IMediator mediator) : Endpoint<AddOrderItemRequest, OrderResponse>
{
    public override void Configure()
    {
        Post("/orders/{id}/items");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Add an item to an order";
            s.Description = "Adds a new item to an existing order";
        });
        Tags("orders");
    }

    public override async Task HandleAsync(AddOrderItemRequest req, CancellationToken ct)
    {
        var orderId = Route<int>("id");
        var command = new AddOrderItemCommand(
            orderId,
            req.ProductId,
            req.ProductName,
            req.Quantity,
            req.UnitPrice
        );

        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // Map UseCases DTO to Endpoint Response DTO
        Response = new OrderResponse(
            result.Value.Id,
            result.Value.CustomerId,
            result.Value.OrderNumber,
            result.Value.OrderDate,
            result.Value.Status,
            result.Value.TotalAmount,
            result.Value.Items.Select(i => new OrderItemResponse(
                i.Id,
                i.ProductId,
                i.ProductName,
                i.Quantity,
                i.UnitPrice,
                i.TotalPrice
            )).ToList()
        );
    }
}