using WebApi.Messaging;
using WebApi.Messaging.Contracts;

namespace WebApi.Infrastructure.IntegrationEventHandlers;

public class CustomerCreatedIntegrationEventHandler : IEventHandler<CustomerCreatedIntegrationEvent>
{
    public CustomerCreatedIntegrationEventHandler() { }

    public Task HandleAsync(
        CustomerCreatedIntegrationEvent @event,
        CancellationToken cancellationToken = default
    )
    {
        Console.WriteLine(
            $"Received CustomerCreatedIntegrationEvent with CustomerId: {@event.CustomerId}, OccurredAt: {@event.OccurredAt}"
        );
        return Task.CompletedTask;
    }
}
