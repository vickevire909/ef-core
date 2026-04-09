using Microsoft.EntityFrameworkCore;
using WebApi.Domain.Enums;
using WebApi.Domain.Interfaces;
using WebApi.Domain.Services;
using WebApi.Infrastructure.Outbox;
using WebApi.Infrastructure.Persistence;
using WebApi.Web;

namespace WebApi;

public static class DependencyInjection
{
    public static IServiceCollection AddWebApi(this IServiceCollection services)
    {
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<ICurrentUser, LoggedInUser>();
        return services;
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddScoped<AuditInfoInterceptor>();
        services.AddScoped<DomainEventsOutboxInterceptor>();
        services.AddDbContext<AppDbContext>(
            (serviceProvider, options) =>
                options
                    .UseNpgsql(
                        "Host=localhost;Database=application;Username=postgres;Password=postgres",
                        o => o.MapEnum<OrderStatus>()
                    )
                    .AddInterceptors(
                        serviceProvider.GetRequiredService<AuditInfoInterceptor>(),
                        serviceProvider.GetRequiredService<DomainEventsOutboxInterceptor>()
                    )
                    .UseSnakeCaseNamingConvention()
        );
        return services;
    }

    public static IServiceCollection AddOutbox(this IServiceCollection services)
    {
        services.AddScoped<MessageTypeCache>();
        services.AddScoped<OutboxProcessor>();
        services.AddHostedService<OutboxBackgroundService>();
        return services;
    }
}
