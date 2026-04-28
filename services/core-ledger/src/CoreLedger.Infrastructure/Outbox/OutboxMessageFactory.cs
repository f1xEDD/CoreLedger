using System.Text.Json;

namespace CoreLedger.Infrastructure.Outbox;

public static class OutboxMessageFactory
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static OutboxMessage From<T>(
        string type,
        T message,
        DateTime occurredAtUtc)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Outbox message type must be provided.", nameof(type));
        }

        var payload = JsonSerializer.Serialize(message, JsonOptions);

        return new OutboxMessage(
            id: Guid.NewGuid(),
            type: type,
            payload: payload,
            occurredAtUtc: occurredAtUtc);
    }
}