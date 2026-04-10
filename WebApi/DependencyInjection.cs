using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WebApi.Domain.Enums;
using WebApi.Domain.Interfaces;
using WebApi.Domain.Services;
using WebApi.Infrastructure.Outbox;
using WebApi.Infrastructure.Outbox.Interfaces;
using WebApi.Infrastructure.Persistence;
using WebApi.Messaging;
using WebApi.Web;

namespace WebApi;

public static class DependencyInjection
{
    public static IServiceCollection AddWebApi(this IServiceCollection services)
    {
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<ICurrentUser, LoggedInUser>();
        services.AddScoped<IntegrationEventCollector>();
        return services;
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddScoped<ISaveChangesInterceptor, AuditInfoInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, IntegrationEventsOutboxInterceptor>();
        services.AddDbContext<AppDbContext>(dbContextConfiguration);
        return services;
    }

    public static OutboxEventHandlerRegistrationBuilder AddOutboxProcessing(
        this IServiceCollection services
    )
    {
        EventHandlerDescriptorRegistry registry = new();
        services.AddSingleton(registry);
        services.AddSingleton<MessageTypeCache>();
        services.AddSingleton<IOutboxMessageDispatcher, InProcessOutboxMessageDispatcher>();
        services.AddScoped<OutboxProcessor>();
        services.AddHostedService<OutboxBackgroundService>();
        return new(services, registry);
    }

    private static readonly Action<
        IServiceProvider,
        DbContextOptionsBuilder
    > dbContextConfiguration = (serviceProvider, options) =>
        options
            .UseNpgsql(
                "Host=localhost;Database=application;Username=postgres;Password=postgres",
                o => o.MapEnum<OrderStatus>()
            )
            .AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>())
            .UseSnakeCaseNamingConvention();
}
