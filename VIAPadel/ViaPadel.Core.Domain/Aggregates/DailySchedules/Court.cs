namespace ViaPadel.Core.Domain.Aggregates.DailySchedules;

/// <summary>Entity inside the DailySchedule aggregate. Identity is its (capitalized) name within the schedule.
/// Holds the per-court invariants; the schedule root drives them and supplies the schedule bounds.</summary>
public sealed class Court : Entity<CourtName>
{
    private Court() { }  
    private readonly List<Booking> _bookings = new();
    public IReadOnlyList<Booking> Bookings => _bookings.AsReadOnly();
    public CourtName Name => Id;

    private Court(CourtName name) : base(name) { }
    internal static Court Named(CourtName name) => new(name);

    /// <summary>UC6 F11: an existing booking on this court overlaps the requested interval.</summary>
    public bool HasOverlap(TimeInterval time) => _bookings.Any(b => b.Time.Overlaps(time));

    /// <summary>UC6 F18: would placing <paramref name="time"/> leave a gap shorter than 60 minutes,
    /// between bookings or against the schedule's start/end?</summary>
    public bool LeavesGapUnderHour(TimeInterval time, TimeInterval scheduleBounds)
    {
        var spans = _bookings.Select(b => b.Time).Append(time).OrderBy(t => t.Start).ToList();
        var cursor = scheduleBounds.Start;
        foreach (var s in spans)
        {
            var gap = (int)(s.Start.ToTimeSpan() - cursor.ToTimeSpan()).TotalMinutes;
            if (gap is > 0 and < 60) return true;
            if (s.End > cursor) cursor = s.End;
        }
        var tail = (int)(scheduleBounds.End.ToTimeSpan() - cursor.ToTimeSpan()).TotalMinutes;
        return tail is > 0 and < 60;
    }

    internal void Add(Booking booking) => _bookings.Add(booking);
    internal void Remove(Booking booking) => _bookings.Remove(booking);
    internal Booking? Find(BookingId id) => _bookings.FirstOrDefault(b => b.Id == id);
}
