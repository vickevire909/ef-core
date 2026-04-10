namespace WebApi.Messaging;

public class CustomerCreatedIntegrationEvent : IntegrationEvent
{
    public required Guid CustomerId { get; init; }
}
