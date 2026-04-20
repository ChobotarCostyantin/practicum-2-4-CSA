using FastEndpoints;
using Mediator;
using Nimble.Modulith.Customers.UseCases.Orders.Commands;

namespace Nimble.Modulith.Customers.Endpoints.Orders;

public class Create(IMediator mediator) : Endpoint<CreateOrderRequest, OrderResponse>
{
    public override void Configure()
    {
        Post("/orders");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new order";
            s.Description = "Creates a new order with the provided items";
        });
        Tags("orders");
    }

    public override async Task HandleAsync(CreateOrderRequest req, CancellationToken ct)
    {
        var command = new CreateOrderCommand(
            req.CustomerId,
            req.OrderDate,
            req.Items.Select(i => new CreateOrderItemDto(
                i.ProductId,
                i.ProductName,
                i.Quantity,
                i.UnitPrice
            )).ToList()
        );

        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // Map UseCases DTO to Endpoint Response DTO
        await Send.CreatedAtAsync<GetById>(
            new { id = result.Value.Id },
            new OrderResponse(
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
            ),
            generateAbsoluteUrl: false,
            cancellation: ct
        );
    }
}