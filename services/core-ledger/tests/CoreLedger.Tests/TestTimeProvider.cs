using CoreLedger.Domain.Time;

namespace CoreLedger.Tests;

public sealed class TestTimeProvider(DateTime utcNow) : ITimeProvider
{
    public DateTime UtcNow => utcNow;
    public DateOnly TodayUtc => DateOnly.FromDateTime(utcNow.Date);
}