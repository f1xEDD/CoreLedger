namespace CoreLedger.Domain.Time;

public static class DateFactory
{
    public static DateOnly Today(ITimeProvider time) => time.TodayUtc;
    public static DateOnly TodayPlusDays(ITimeProvider time, int days) => time.TodayUtc.AddDays(days);
    public static DateOnly FromUtc(DateTime utc) => DateOnly.FromDateTime(utc);
}