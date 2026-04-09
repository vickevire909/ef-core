namespace WebApi.Infrastructure.Outbox;

public class OutboxBackgroundService : BackgroundService
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<OutboxBackgroundService> logger;
    private int consecutiveFailures = 0;

    public OutboxBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<OutboxBackgroundService> logger
    )
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox background service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var outboxProcessor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();
                await outboxProcessor.ProcessBatchAsync(stoppingToken);

                // Reset failure counter on success
                consecutiveFailures = 0;
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
                logger.LogInformation("Outbox background service stopping");
                break;
            }
            catch (Exception ex)
            {
                consecutiveFailures++;
                logger.LogWarning(
                    ex,
                    "Error processing outbox batch (attempt {Attempt})",
                    consecutiveFailures
                );

                // Exponential backoff: 5s, 10s, 20s, 40s, up to 5 minutes
                var delaySeconds = Math.Min(
                    TimeSpan.FromSeconds(3 * Math.Pow(2, consecutiveFailures - 1)).TotalSeconds,
                    300 // 5 minutes max
                );

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    logger.LogInformation("Outbox background service stopping");
                    break;
                }

                // Reset after 10 consecutive failures to avoid getting stuck
                if (consecutiveFailures >= 10)
                {
                    logger.LogWarning(
                        "Resetting consecutive failures counter after {Count} attempts",
                        consecutiveFailures
                    );
                    consecutiveFailures = 0;
                }
            }
        }

        logger.LogInformation("Outbox background service terminated");
    }
}
