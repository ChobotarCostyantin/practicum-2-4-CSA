using FastEndpoints;
using Mediator;
using Nimble.Modulith.Customers.UseCases.Orders.Queries;

namespace Nimble.Modulith.Customers.Endpoints.Orders;

public class GetById(IMediator mediator) : EndpointWithoutRequest<OrderResponse>
{
    public override void Configure()
    {
        Get("/orders/{id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get an order by ID";
            s.Description = "Returns a single order with all its items";
        });
        Tags("orders");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");
        var query = new GetOrderByIdQuery(id);
        var result = await mediator.Send(query, ct);

        if (!result.IsSuccess)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

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