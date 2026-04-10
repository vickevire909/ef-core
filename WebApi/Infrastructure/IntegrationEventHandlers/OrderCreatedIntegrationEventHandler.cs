using Microsoft.EntityFrameworkCore;
using WebApi.Infrastructure.Persistence;
using WebApi.Messaging;
using WebApi.Messaging.Contracts;

namespace WebApi.Infrastructure.IntegrationEventHandlers;

public class OrderCreatedIntegrationEventHandler : IEventHandler<OrderCreatedIntegrationEvent>
{
    private readonly AppDbContext appDbContext;

    public OrderCreatedIntegrationEventHandler(AppDbContext appDbContext)
    {
        this.appDbContext = appDbContext;
    }

    public async Task HandleAsync(
        OrderCreatedIntegrationEvent @event,
        CancellationToken cancellationToken = default
    )
    {
        Console.WriteLine(
            $"Received OrderCreatedIntegrationEvent with OrderId: {@event.OrderId}, OccurredAt: {@event.OccurredAt}"
        );

        var order = await appDbContext.Orders.FirstOrDefaultAsync(
            o => o.Id == @event.OrderId,
            cancellationToken
        );
        Console.WriteLine(
            $"Queried Order from DB: Id: {order?.Id}, Description: {order?.Description}"
        );
    }
}
