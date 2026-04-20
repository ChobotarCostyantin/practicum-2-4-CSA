using Ardalis.Result;
using Mediator;
using Nimble.Modulith.Customers.Domain.CustomerAggregate;
using Nimble.Modulith.Customers.Domain.Interfaces;
using Nimble.Modulith.Customers.Domain.OrderAggregate;

namespace Nimble.Modulith.Customers.UseCases.Orders.Commands;

public class CreateOrderHandler(
    IRepository<Order> orderRepository,
    IReadRepository<Customer> customerRepository) 
    : ICommandHandler<CreateOrderCommand, Result<OrderDto>>
{
    public async ValueTask<Result<OrderDto>> Handle(CreateOrderCommand command, CancellationToken ct)
    {
        // Verify customer exists
        var customer = await customerRepository.GetByIdAsync(command.CustomerId, ct);
        if (customer is null)
        {
            return Result<OrderDto>.NotFound($"Customer with ID {command.CustomerId} not found");
        }

        // Create order entity
        var order = new Order
        {
            CustomerId = command.CustomerId,
            OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}",
            OrderDate = command.OrderDate,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // Add order items
        foreach (var itemDto in command.Items)
        {
            var item = new OrderItem
            {
                ProductId = itemDto.ProductId,
                ProductName = itemDto.ProductName,
                Quantity = itemDto.Quantity,
                UnitPrice = itemDto.UnitPrice
            };
            order.AddItem(item);
        }

        await orderRepository.AddAsync(order, ct);
        await orderRepository.SaveChangesAsync(ct);

        // Map to DTO
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