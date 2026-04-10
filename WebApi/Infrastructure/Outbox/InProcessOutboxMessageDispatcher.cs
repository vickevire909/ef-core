using System.Diagnostics.CodeAnalysis;
using WebApi.Infrastructure.Outbox.Interfaces;

namespace WebApi.Infrastructure.Outbox;

public class InProcessOutboxMessageDispatcher : IOutboxMessageDispatcher
{
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly EventHandlerDescriptorRegistry registry;

    public InProcessOutboxMessageDispatcher(
        IServiceScopeFactory serviceScopeFactory,
        EventHandlerDescriptorRegistry registry
    )
    {
        this.serviceScopeFactory = serviceScopeFactory;
        this.registry = registry;
    }

    public Task DispatchAsync(string payload, Type type, CancellationToken cancellationToken)
    {
        if (!registry.TryGetDescriptor(type, out var descriptor))
        {
            throw new InvalidOperationException($"No handler found for event type {type}");
        }

        return descriptor.Handle(serviceScopeFactory, payload, cancellationToken);
    }
}
