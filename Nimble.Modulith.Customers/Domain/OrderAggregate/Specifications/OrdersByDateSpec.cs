using Ardalis.Specification;

namespace Nimble.Modulith.Customers.Domain.OrderAggregate.Specifications;

public class OrdersByDateSpec : Specification<Order>
{
    public OrdersByDateSpec(DateOnly orderDate)
    {
        Query
            .Where(o => o.OrderDate == orderDate)
            .Include(o => o.Items)
            .OrderBy(o => o.CreatedAt);
    }
}