namespace WebApi.Messaging;

public class IntegrationEventCollector
{
    private readonly List<IntegrationEvent> events = [];

    public void Add(IntegrationEvent @event)
    {
        events.Add(@event);
    }

    public List<IntegrationEvent> ClearEvents()
    {
        List<IntegrationEvent> eventsToReturn = [.. events];
        events.Clear();
        return eventsToReturn;
    }
}
