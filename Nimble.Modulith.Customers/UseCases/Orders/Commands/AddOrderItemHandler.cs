using Ardalis.Result;
using Mediator;
using Nimble.Modulith.Customers.Domain.Interfaces;
using Nimble.Modulith.Customers.Domain.OrderAggregate;
using Nimble.Modulith.Customers.Domain.OrderAggregate.Specifications;

namespace Nimble.Modulith.Customers.UseCases.Orders.Commands;

public class AddOrderItemHandler(IRepository<Order> repository) 
    : ICommandHandler<AddOrderItemCommand, Result<OrderDto>>
{
    public async ValueTask<Result<OrderDto>> Handle(AddOrderItemCommand command, CancellationToken ct)
    {
        var spec = new OrderByIdSpec(command.OrderId);
        var order = await repository.SingleOrDefaultAsync(spec, ct);

        if (order is null)
        {
            return Result<OrderDto>.NotFound();
        }

        var item = new OrderItem
        {
            ProductId = command.ProductId,
            ProductName = command.ProductName,
            Quantity = command.Quantity,
            UnitPrice = command.UnitPrice
        };

        order.AddItem(item);
        order.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(order, ct);
        await repository.SaveChangesAsync(ct);

        var dto = new OrderDto(
            order.Id,
            order.CustomerId,
            order.OrderNumber,
            order.OrderDate,
            order.Status.ToString(),
            order.TotalAmount,
            order.Items.Select(i => new OrderItemDto(
                i.Id,
                i.ProductId,
                i.ProductName,
                i.Quantity,
                i.UnitPrice,
                i.TotalPrice
            )).ToList(),
            order.CreatedAt,
            order.UpdatedAt
        );

        return Result<OrderDto>.Success(dto);
    }
}