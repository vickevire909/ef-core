using WebApi.Messaging.Contracts;

namespace WebApi.Messaging;

public abstract class IntegrationEvent : IEvent
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public required DateTime OccurredAt { get; init; }
}
