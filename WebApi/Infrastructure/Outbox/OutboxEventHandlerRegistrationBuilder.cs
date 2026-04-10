using WebApi.Messaging.Contracts;

namespace WebApi.Infrastructure.Outbox;

public class OutboxEventHandlerRegistrationBuilder
{
    private readonly IServiceCollection services;
    private readonly EventHandlerDescriptorRegistry registry;

    public OutboxEventHandlerRegistrationBuilder(
        IServiceCollection services,
        EventHandlerDescriptorRegistry registry
    )
    {
        this.services = services;
        this.registry = registry;
    }

    public OutboxEventHandlerRegistrationBuilder WithEventHandler<THandler, TEvent>()
        where THandler : class, IEventHandler<TEvent>
        where TEvent : IEvent
    {
        services.AddScoped<THandler>();
        registry.AddDescriptor(EventHandlerDescriptor.Create<TEvent, THandler>());
        return this;
    }
}
