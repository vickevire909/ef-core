namespace WebApi.Domain.Events;

public abstract class DomainEvent
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
}
