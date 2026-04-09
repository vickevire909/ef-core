using System;

namespace WebApi.Domain.Events;

public class OrderCreatedEvent : DomainEvent
{
    public Guid OrderId { get; set; }
    public DateTime CreatedAt { get; set; }
}
