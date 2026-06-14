namespace ViaPadel.Core.Application.Common;

/// <summary>Shared time helpers used across booking / schedule / VIP rules.</summary>
public static class TimeRules
{
    /// <summary>True when minutes are :00 or :30 and there are no seconds.</summary>
    public static bool IsWholeOrHalfHour(TimeOnly t) => (t.Minute is 0 or 30) && t.Second == 0 && t.Millisecond == 0;

    /// <summary>Duration in whole minutes (assumes end &gt;= start, same day).</summary>
    public static double DurationMinutes(TimeOnly start, TimeOnly end) => (end.ToTimeSpan() - start.ToTimeSpan()).TotalMinutes;

    /// <summary>Half-open overlap test: [aStart,aEnd) intersects [bStart,bEnd).</summary>
    public static bool Overlaps(TimeOnly aStart, TimeOnly aEnd, TimeOnly bStart, TimeOnly bEnd) => aStart < bEnd && bStart < aEnd;
}
