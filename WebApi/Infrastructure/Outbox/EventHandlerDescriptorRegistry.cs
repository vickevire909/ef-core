using System.Diagnostics.CodeAnalysis;

namespace WebApi.Infrastructure.Outbox;

/// <summary>
/// Registry for event handler descriptors, used to look up the appropriate handler for a given event type during outbox message processing.
/// </summary>
public class EventHandlerDescriptorRegistry
{
    private readonly Dictionary<string, EventHandlerDescriptor> descriptors = [];

    public void AddDescriptor(EventHandlerDescriptor descriptor) =>
        descriptors[descriptor.EventType.FullName!] = descriptor;

    public bool TryGetDescriptor(
        Type eventType,
        [NotNullWhen(true)] out EventHandlerDescriptor? descriptor
    ) => descriptors.TryGetValue(eventType.FullName!, out descriptor);
}
