using Microsoft.EntityFrameworkCore;
using WebApi.Infrastructure.Outbox.Interfaces;
using WebApi.Infrastructure.Persistence;

namespace WebApi.Infrastructure.Outbox;

public class OutboxProcessor
{
    private readonly ILogger<OutboxProcessor> logger;
    private readonly IOutboxMessageDispatcher outboxMessageDispatcher;
    private readonly MessageTypeCache messageTypeCache;
    private readonly AppDbContext appDbContext;

    public OutboxProcessor(
        ILogger<OutboxProcessor> logger,
        IOutboxMessageDispatcher outboxMessageDispatcher,
        MessageTypeCache messageTypeCache,
        AppDbContext appDbContext
    )
    {
        this.logger = logger;
        this.outboxMessageDispatcher = outboxMessageDispatcher;
        this.messageTypeCache = messageTypeCache;
        this.appDbContext = appDbContext;
    }

    public int MaxRetries { get; private set; } = 5;
    public int BatchSize { get; private set; } = 100;

    public async Task ProcessBatchAsync(CancellationToken cancellationToken = default)
    {
        await using var tx = await appDbContext.Database.BeginTransactionAsync(cancellationToken);

        var batch = await appDbContext
            .OutboxMessages.FromSql(
                $"""
                SELECT * FROM outbox_messages
                WHERE retry_count < {MaxRetries}
                ORDER BY "created_at" ASC
                LIMIT {BatchSize}
                FOR UPDATE SKIP LOCKED
                """
            )
            .ToListAsync(cancellationToken);

        if (batch.Count == 0)
        {
            await tx.RollbackAsync(cancellationToken);
            return;
        }

        var succeeded = new List<Guid>();

        foreach (var message in batch)
        {
            try
            {
                var type = messageTypeCache.GetType(message.Type);
                await outboxMessageDispatcher.DispatchAsync(
                    message.Payload,
                    type,
                    cancellationToken
                );
                succeeded.Add(message.Id);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to dispatch outbox message {Id}", message.Id);
            }
        }

        // Delete successes
        if (succeeded.Count != 0)
        {
            await appDbContext
                .OutboxMessages.Where(m => succeeded.Contains(m.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }

        // Increment retry count on failures
        var failed = batch.Select(m => m.Id).Except(succeeded).ToList();
        if (failed.Count != 0)
        {
            await appDbContext
                .OutboxMessages.Where(m => failed.Contains(m.Id))
                .ExecuteUpdateAsync(
                    s => s.SetProperty(m => m.RetryCount, m => m.RetryCount + 1),
                    cancellationToken
                );
        }

        await tx.CommitAsync(cancellationToken);
    }
}
