using System.Collections.Concurrent;

namespace WebApi.Infrastructure.Outbox;

public class MessageTypeCache
{
    private readonly ConcurrentDictionary<string, Type> cache = new();

    public Type GetType(string typeName)
    {
        return cache.GetOrAdd(
            typeName,
            name =>
            {
                var type = Type.GetType(name);
                return type ?? throw new InvalidOperationException($"Type '{name}' not found.");
            }
        );
    }
}
