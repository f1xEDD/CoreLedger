namespace CoreLedger.Domain.Time;

public interface ITimeProvider
{
    DateTime UtcNow { get; }
    DateOnly TodayUtc { get; }
}