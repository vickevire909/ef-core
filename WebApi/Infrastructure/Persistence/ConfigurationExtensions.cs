using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApi.Domain.Entities;

namespace WebApi.Infrastructure.Persistence;

public static class ConfigurationExtensions
{
    public static void ApplyEntityBaseConfiguration<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        bool includeAuditFields = true
    )
        where TEntity : Entity
    {
        entityTypeBuilder.HasKey(e => e.Id);
        entityTypeBuilder.Property(e => e.Id).ValueGeneratedNever().HasColumnOrder(0);
        if (includeAuditFields)
        {
            entityTypeBuilder
                .Property<DateTime>(AuditField.CreatedAt)
                .IsRequired()
                .HasColumnOrder(1);
            entityTypeBuilder.Property<string>(AuditField.CreatedBy).IsRequired().HasColumnOrder(2);
            entityTypeBuilder
                .Property<DateTime>(AuditField.UpdatedAt)
                .IsRequired()
                .HasColumnOrder(3);
            entityTypeBuilder.Property<string>(AuditField.UpdatedBy).IsRequired().HasColumnOrder(4);
        }
    }

    public static void ApplyAggregateRootBaseConfiguration<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        bool includeAuditFields = true
    )
        where TEntity : AggregateRoot
    {
        entityTypeBuilder.ApplyEntityBaseConfiguration(includeAuditFields);
        entityTypeBuilder.Ignore(e => e.DomainEvents);
    }
}
