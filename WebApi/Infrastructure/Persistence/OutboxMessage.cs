using System.Text.Json;

namespace WebApi.Infrastructure.Persistence;

public class OutboxMessage
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public string Type { get; private set; } = null!;
    public string Payload { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public int RetryCount { get; set; } = 0;

    private OutboxMessage() { }

    public static OutboxMessage Create<T>(T content, DateTime createdAt)
        where T : class
    {
        Type contentType = content.GetType();
        var type =
            contentType.AssemblyQualifiedName
            ?? throw new InvalidOperationException("Unable to determine the type of the content.");
        var payload = JsonSerializer.Serialize(content, contentType);
        return new OutboxMessage
        {
            Type = type,
            Payload = payload,
            CreatedAt = createdAt,
        };
    }
}
