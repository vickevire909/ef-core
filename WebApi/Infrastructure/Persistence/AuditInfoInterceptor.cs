using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WebApi.Domain.Entities;
using WebApi.Domain.Interfaces;
using WebApi.Domain.Services;

namespace WebApi.Infrastructure.Persistence;

public class AuditInfoInterceptor : SaveChangesInterceptor
{
    private readonly IDateTimeProvider dateTimeProvider;
    private readonly ICurrentUser currentUser;

    public AuditInfoInterceptor(IDateTimeProvider dateTimeProvider, ICurrentUser currentUser)
    {
        this.dateTimeProvider = dateTimeProvider;
        this.currentUser = currentUser;
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
        string username = currentUser.GetName();
        DateTime utcNow = dateTimeProvider.UtcNow;
        foreach (var entry in appDbContext.ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Metadata.FindProperty(AuditField.CreatedBy) != null)
                {
                    entry.Property<string>(AuditField.CreatedBy).CurrentValue = username;
                }
                if (entry.Metadata.FindProperty(AuditField.CreatedAt) != null)
                {
                    entry.Property<DateTime>(AuditField.CreatedAt).CurrentValue = utcNow;
                }
            }
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                if (entry.Metadata.FindProperty(AuditField.UpdatedBy) != null)
                {
                    entry.Property<string>(AuditField.UpdatedBy).CurrentValue = username;
                }
                if (entry.Metadata.FindProperty(AuditField.UpdatedAt) != null)
                {
                    entry.Property<DateTime>(AuditField.UpdatedAt).CurrentValue = utcNow;
                }
            }
        }
    }
}
