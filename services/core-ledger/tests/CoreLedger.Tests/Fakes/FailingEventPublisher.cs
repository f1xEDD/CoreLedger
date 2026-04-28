using CoreLedger.Application.Outbox;

namespace CoreLedger.Tests.Fakes;

public sealed class FailingEventPublisher : IEventPublisher
{
    public Task PublishAsync(string type, string payload, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Publish failed");
    }
}