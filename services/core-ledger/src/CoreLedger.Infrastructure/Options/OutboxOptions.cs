namespace CoreLedger.Infrastructure.Options;

public sealed class OutboxOptions
{
    public int BatchSize { get; init; } = 20;

    public int PollingIntervalSeconds { get; init; } = 5;

    public int MaxAttempts { get; init; } = 5;
}