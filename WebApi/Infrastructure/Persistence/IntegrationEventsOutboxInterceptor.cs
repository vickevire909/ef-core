using Microsoft.EntityFrameworkCore.Diagnostics;
using WebApi.Domain.Entities;
using WebApi.Domain.Services;
using WebApi.Messaging;

namespace WebApi.Infrastructure.Persistence;

public class IntegrationEventsOutboxInterceptor : SaveChangesInterceptor
{
    private readonly IDateTimeProvider dateTimeProvider;
    private readonly IntegrationEventCollector integrationEventCollector;

    public IntegrationEventsOutboxInterceptor(
        IDateTimeProvider dateTimeProvider,
        IntegrationEventCollector integrationEventCollector
    )
    {
        this.dateTimeProvider = dateTimeProvider;
        this.integrationEventCollector = integrationEventCollector;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        Intercept(eventData);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result
    )
    {
        Intercept(eventData);
        return base.SavingChanges(eventData, result);
    }

    private void Intercept(DbContextEventData eventData)
    {
        if (eventData.Context is not AppDbContext appDbContext)
        {
            return;
        }
        List<IntegrationEvent> integrationEvents = integrationEventCollector.ClearEvents();
        if (integrationEvents.Count == 0)
        {
            return;
        }
        foreach (var integrationEvent in integrationEvents)
        {
            appDbContext.OutboxMessages.Add(
                OutboxMessage.Create(integrationEvent, dateTimeProvider.UtcNow)
            );
        }
    }
}
