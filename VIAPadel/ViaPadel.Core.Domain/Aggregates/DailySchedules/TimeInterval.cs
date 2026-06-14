using ViaPadel.Core.Tools.OperationResult;

namespace ViaPadel.Core.Domain.Aggregates.DailySchedules;

/// <summary>A half-open range [Start, End). Enforces the rules common to ALL intervals: minutes must be
/// :00 or :30, and end after start. Duration thresholds (60 min schedule/booking, 30 min VIP) are the
/// caller's concern, since they differ by context.</summary>
public sealed class TimeInterval : ValueObject
{
    private TimeInterval() { }
    public TimeOnly Start { get; }
    public TimeOnly End { get; }
    private TimeInterval(TimeOnly start, TimeOnly end) { Start = start; End = end; }

    public static Result<TimeInterval> Create(TimeOnly start, TimeOnly end)
    {
        if (!IsHalfOrWholeHour(start) || !IsHalfOrWholeHour(end))
            return new ResultError("time.minutes", "The minutes must be half or whole hours (:00 or :30)."); // UC2 F6 / UC6 F9 / UC13 F4
        if (end <= start)
            return new ResultError("time.order", "The end time must be after the start time.");              // UC2 F3
        return new TimeInterval(start, end);
    }

    internal static TimeInterval Trusted(TimeOnly start, TimeOnly end) => new(start, end);

    public int DurationMinutes() => (int)(End.ToTimeSpan() - Start.ToTimeSpan()).TotalMinutes;
    public bool Overlaps(TimeInterval other) => Start < other.End && other.Start < End;
    public bool Contains(TimeInterval other) => Start <= other.Start && other.End <= End;

    private static bool IsHalfOrWholeHour(TimeOnly t)
        => t.Minute is 0 or 30 && t.Second == 0 && t.Millisecond == 0;

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Start; yield return End; }
    public override string ToString() => $"{Start:HH\\:mm}-{End:HH\\:mm}";
    public static TimeInterval From(TimeOnly start, TimeOnly end) => new(start, end);
}
