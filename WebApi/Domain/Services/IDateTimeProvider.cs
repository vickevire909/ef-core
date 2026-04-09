namespace WebApi.Domain.Services;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
