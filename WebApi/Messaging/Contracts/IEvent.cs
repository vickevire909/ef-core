namespace WebApi.Messaging.Contracts;

public interface IEvent
{
    public Guid Id { get; }
    public DateTime OccurredAt { get; init; }
}
