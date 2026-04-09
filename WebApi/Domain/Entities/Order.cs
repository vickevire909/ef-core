using WebApi.Domain.Enums;
using WebApi.Domain.Events;

namespace WebApi.Domain.Entities;

public class Order : AggregateRoot
{
    public string Description { get; set; } = null!;
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public DateTime StatusAt { get; set; }

    private Order() { }

    public static Order Create(string description, DateTime createdAt)
    {
        var order = new Order
        {
            Description = description,
            Status = OrderStatus.Pending,
            StatusAt = createdAt,
        };

        order.RaiseEvent(new OrderCreatedEvent { OrderId = order.Id, CreatedAt = createdAt });

        return order;
    }
}
