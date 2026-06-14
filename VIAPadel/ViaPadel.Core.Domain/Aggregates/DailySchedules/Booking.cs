using ViaPadel.Core.Domain.Aggregates.Players;

namespace ViaPadel.Core.Domain.Aggregates.DailySchedules;

/// <summary>Entity inside the DailySchedule aggregate. References the booking player by identity only.</summary>
public sealed class Booking : Entity<BookingId>
{
    private Booking () {}
    public TimeInterval Time { get; private set; }
    public PlayerId Player { get; private set; }

    private Booking(BookingId id, TimeInterval time, PlayerId player) : base(id)
    {
        Time = time;
        Player = player;
    }

    internal static Booking Create(TimeInterval time, PlayerId player) => new(BookingId.New(), time, player);
}
