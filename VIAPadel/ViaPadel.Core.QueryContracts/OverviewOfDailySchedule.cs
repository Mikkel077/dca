namespace ViaPadel.Core.QueryContracts;

public abstract class OverviewOfDailySchedule
{
    public record Query(DateOnly Date) : IQuery<OverviewOfDailySchedule.Answer>;

    public record Answer(
        DateOnly Date,
        List<TimeSlotInfo> TimeSlots,
        List<CourtScheduleInfo> Courts
    );

    public record CourtScheduleInfo(
        string CourtName,
        string DisplayName,
        List<BookingInfo> Bookings
    );

    public record TimeSlotInfo(TimeOnly StartTime, TimeOnly EndTime, bool IsVipOnly);

    public record BookingInfo(
        Guid BookingId,
        TimeOnly StartTime,
        TimeOnly EndTime,
        string BookedByName
    );
}