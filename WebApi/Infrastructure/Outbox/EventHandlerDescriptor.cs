using System.Text.Json;
using WebApi.Messaging.Contracts;

namespace WebApi.Infrastructure.Outbox;

/// <summary>
/// Descriptor for an event handler, containing the event type and a function to handle the event given a payload and cancellation token. Used during outbox message processing to invoke the correct handler for each event type.
/// </summary>
public class EventHandlerDescriptor
{
    public Type EventType { get; }
    public Func<IServiceScopeFactory, string, CancellationToken, Task> Handle { get; }

    private EventHandlerDescriptor(
        Type eventType,
        Func<IServiceScopeFactory, string, CancellationToken, Task> handle
    )
    {
        EventType = eventType;
        Handle = handle;
    }

    public static EventHandlerDescriptor Create<TEvent, THandler>()
        where TEvent : IEvent
        where THandler : IEventHandler<TEvent>
    {
        return new EventHandlerDescriptor(
            eventType: typeof(TEvent),
            handle: async (scopeFactory, payload, cancellationToken) =>
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<THandler>();
                var @event =
                    JsonSerializer.Deserialize<TEvent>(payload)
                    ?? throw new InvalidOperationException(
                        $"Failed to deserialize event of type {typeof(TEvent).FullName}"
                    );
                await handler.HandleAsync(@event, cancellationToken);
            }
        );
    }
}
