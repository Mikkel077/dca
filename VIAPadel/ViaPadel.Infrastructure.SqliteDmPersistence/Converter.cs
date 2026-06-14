using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ViaPadel.Core.Domain.Aggregates.DailySchedules;
using ViaPadel.Core.Domain.Aggregates.Players;

namespace ViaPadel.Infrastructure.SqliteDmPersistence;

public sealed class CourtNameConverter : ValueConverter<CourtName, string>
{
    public CourtNameConverter() : base(n => n.Value, v => CourtName.From(v)) { }
}

public sealed class BookingIdConverter : ValueConverter<BookingId, Guid>
{
    public BookingIdConverter() : base(id => id.Value, v => BookingId.From(v)) { }
}

public sealed class PlayerIdConverter : ValueConverter<PlayerId, Guid>
{
    public PlayerIdConverter() : base(id => id.Value, v => PlayerId.From(v)) { }
}

public sealed class ScheduleIdConverter : ValueConverter<ScheduleId, Guid>
{
    public ScheduleIdConverter() : base(id => id.Value, v => ScheduleId.From(v)) { }
}

public sealed class TimeIntervalConverter : ValueConverter<TimeInterval, string>
{
    public TimeIntervalConverter() : base(t => SerializeTime(t), s => DeserializeTime(s)) { }

    private static string SerializeTime(TimeInterval t) => t.Start.Ticks + "|" + t.End.Ticks;
    private static TimeInterval DeserializeTime(string s)
    {
        var parts = s.Split('|');
        return TimeInterval.From(new TimeOnly(long.Parse(parts[0])),
            new TimeOnly(long.Parse(parts[1])));
    }
}