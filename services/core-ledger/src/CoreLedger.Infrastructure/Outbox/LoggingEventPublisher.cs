using CoreLedger.Application.Outbox;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Infrastructure.Outbox;

public sealed class LoggingEventPublisher(ILogger<LoggingEventPublisher> logger) : IEventPublisher
{
    public Task PublishAsync(
        string type,
        string payload,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Published outbox event: type={EventType}, payload={Payload}",
            type,
            payload);

        return Task.CompletedTask;
    }
}