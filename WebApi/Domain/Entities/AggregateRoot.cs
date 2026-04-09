using WebApi.Domain.Events;

namespace WebApi.Domain.Entities;

public class AggregateRoot : Entity
{
    private readonly List<DomainEvent> domainEvents = [];
    public IReadOnlyCollection<DomainEvent> DomainEvents => domainEvents.AsReadOnly();

    protected void RaiseEvent(DomainEvent domainEvent)
    {
        domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears the domain events and returns them.
    /// </summary>
    /// <returns></returns>
    public IReadOnlyList<DomainEvent> ClearEvents()
    {
        List<DomainEvent> events = [.. domainEvents];
        domainEvents.Clear();
        return events;
    }
}
