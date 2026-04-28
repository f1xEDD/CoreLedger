using CoreLedger.Application.Outbox;

namespace CoreLedger.Tests.Fakes;

public sealed class FakeEventPublisher : IEventPublisher
{
    public List<(string Type, string Payload)> Published { get; } = [];

    public Task PublishAsync(string type, string payload, CancellationToken cancellationToken)
    {
        Published.Add((type, payload));
        return Task.CompletedTask;
    }
}