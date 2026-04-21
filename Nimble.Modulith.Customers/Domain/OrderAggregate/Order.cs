using System.Diagnostics.CodeAnalysis;
using Nimble.Modulith.Customers.Domain.Common;

namespace Nimble.Modulith.Customers.Domain.OrderAggregate;

public class Order : EntityBase
{
    private readonly List<OrderItem> _items = new();

    public int CustomerId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateOnly OrderDate { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    public void AddItem(OrderItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (Status != OrderStatus.Pending) throw new InvalidOperationException("Cannot modify confirmed order");

        // Check if an item with the same product already exists
        var existingItem = _items.FirstOrDefault(i => i.ProductId == item.ProductId);
        
        if (existingItem != null)
        {
            // Combine quantities for existing product
            existingItem.Quantity += item.Quantity;
        }
        else
        {
            _items.Add(item);
        }
    }

    public void RemoveItem(OrderItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (Status != OrderStatus.Pending) throw new InvalidOperationException("Cannot modify confirmed order");
        _items.Remove(item);
    }
}