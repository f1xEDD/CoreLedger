using CoreLedger.Application.Outbox;
using CoreLedger.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoreLedger.Infrastructure.Outbox;

public sealed class OutboxDispatcher(
    IServiceScopeFactory scopeFactory,
    IOptions<OutboxOptions> options,
    ILogger<OutboxDispatcher> logger) : BackgroundService
{
    private readonly OutboxOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Outbox dispatcher started. batch_size={BatchSize}, polling_interval_seconds={Interval}",
            _options.BatchSize,
            _options.PollingIntervalSeconds);

        using var timer = new PeriodicTimer(
            TimeSpan.FromSeconds(_options.PollingIntervalSeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // normal shutdown
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox dispatcher iteration failed.");
            }

            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        logger.LogInformation("Outbox dispatcher stopped.");
    }

    public async Task DispatchOnceAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<LedgerDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        var messages = await db.OutboxMessages
            .Where(x =>
                x.Status == OutboxMessageStatus.Pending &&
                x.Attempts < _options.MaxAttempts)
            .OrderBy(x => x.OccurredAtUtc)
            .Take(_options.BatchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            return;
        }

        logger.LogInformation(
            "Outbox dispatcher picked {Count} message(s).",
            messages.Count);

        foreach (var message in messages)
        {
            try
            {
                await publisher.PublishAsync(
                    message.Type,
                    message.Payload,
                    cancellationToken);

                message.MarkProcessed(DateTime.UtcNow);

                logger.LogInformation(
                    "Outbox message processed: message_id={MessageId}, type={EventType}",
                    message.Id,
                    message.Type);
            }
            catch (Exception ex)
            {
                if (message.Attempts + 1 >= _options.MaxAttempts)
                {
                    message.MarkFailed(ex.Message);

                    logger.LogError(
                        ex,
                        "Outbox message failed permanently: message_id={MessageId}, type={EventType}, attempts={Attempts}",
                        message.Id,
                        message.Type,
                        message.Attempts);
                }
                else
                {
                    message.MarkPending(ex.Message);

                    logger.LogWarning(
                        ex,
                        "Outbox message publish failed and will be retried: message_id={MessageId}, type={EventType}, attempts={Attempts}",
                        message.Id,
                        message.Type,
                        message.Attempts);
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}