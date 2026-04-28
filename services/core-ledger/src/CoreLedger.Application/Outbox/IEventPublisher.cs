namespace CoreLedger.Application.Outbox;

public interface IEventPublisher
{
    Task PublishAsync(
        string type,
        string payload,
        CancellationToken cancellationToken);
}