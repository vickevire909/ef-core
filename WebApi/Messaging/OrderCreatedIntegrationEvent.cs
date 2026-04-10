namespace WebApi.Messaging;

public class OrderCreatedIntegrationEvent : IntegrationEvent
{
    public required Guid OrderId { get; init; }
}
