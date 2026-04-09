using Microsoft.EntityFrameworkCore.Diagnostics;
using WebApi.Domain.Entities;
using WebApi.Domain.Services;

namespace WebApi.Infrastructure.Persistence;

public class DomainEventsOutboxInterceptor : SaveChangesInterceptor
{
    private readonly IDateTimeProvider dateTimeProvider;

    public DomainEventsOutboxInterceptor(IDateTimeProvider dateTimeProvider)
    {
        this.dateTimeProvider = dateTimeProvider;
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
        if (eventData.Context is AppDbContext appDbContext)
        {
            foreach (var entry in appDbContext.ChangeTracker.Entries<AggregateRoot>().ToList())
            {
                var entity = entry.Entity;
                var domainEvents = entity.ClearEvents();
                if (domainEvents.Count > 0)
                {
                    foreach (var domainEvent in domainEvents)
                    {
                        appDbContext.OutboxMessages.Add(
                            OutboxMessage.Create(domainEvent, dateTimeProvider.UtcNow)
                        );
                    }
                }
            }
        }
    }
}
