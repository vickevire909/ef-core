namespace WebApi.Infrastructure.Outbox.Interfaces;

public interface IOutboxMessageDispatcher
{
    Task DispatchAsync(string payload, Type type, CancellationToken cancellationToken);
}
