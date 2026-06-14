using ViaPadel.Core.Domain.Aggregates.Players;
using ViaPadel.Core.Tools.OperationResult;

namespace ViaPadel.Core.Domain.Aggregates.DailySchedules;

public sealed class DailySchedule : AggregateRoot<ScheduleId>
{
    private DailySchedule(){}
    private static readonly TimeInterval DefaultTime =
        TimeInterval.Trusted(new TimeOnly(15, 0), new TimeOnly(22, 0));

    private readonly List<Court> _courts = new();
    private readonly List<TimeInterval> _vipSpans = new();

    public TimeInterval Time { get; private set; }
    public DateOnly Date { get; private set; }
    public ScheduleStatus Status { get; private set; }
    public Queue NotificationQueue { get; private set; }
    public IReadOnlyList<Court> Courts => _courts.AsReadOnly();
    public IReadOnlyList<TimeInterval> VipSpans => _vipSpans.AsReadOnly();

    private DailySchedule(ScheduleId id, DateOnly date, TimeInterval time) : base(id)
    {
        Date = date;
        Time = time;
        Status = ScheduleStatus.Draft;
        NotificationQueue = Queue.Empty();
    }

    
    public static DailySchedule Create(DateOnly today) => new(ScheduleId.New(), today, DefaultTime);

    
    public Result<None> UpdateDateAndTime(DateOnly date, TimeInterval time, DateOnly today)
    {
        if (Status != ScheduleStatus.Draft)
            return new ResultError("schedule.notDraft", "An active or deleted schedule cannot be modified, only deleted."); // F5
        if (date < today)
            return new ResultError("schedule.past", "A daily schedule cannot be set in the past.");                          // F2
        if (time.DurationMinutes() < 60)
            return new ResultError("time.tooShort", "The time interval must span 60 minutes or more.");                      // F4
        Date = date;
        Time = time;
        return new None();
    }

    
    public Result<None> AddCourt(CourtName name, DateOnly today)
    {
        if (Status == ScheduleStatus.Deleted)
            return new ResultError("schedule.deleted", "Deleted schedules cannot be updated.");                 // F3
        if (Date < today)
            return new ResultError("schedule.past", "Past daily schedules cannot be updated.");                 // F1
        if (_courts.Any(c => c.Name == name))
            return new ResultError("court.duplicate", "This court is already added to the daily schedule.");    // F7
        _courts.Add(Court.Named(name));                                                                         // S1-S3
        return new None();
    }
    
    public Result<None> Activate(DateTime now)
    {
        if (Status == ScheduleStatus.Deleted)
            return new ResultError("schedule.deleted", "A deleted daily schedule cannot be activated.");        // F3
        if (Status == ScheduleStatus.Active)
            return new ResultError("schedule.alreadyActive", "The schedule is already active.");                // F6
        if (_courts.Count < 1)
            return new ResultError("schedule.noCourts", "A daily schedule without courts cannot be activated.");// F1
        if (Date.ToDateTime(Time.Start) <= now)
            return new ResultError("schedule.pastStart", "A daily schedule with a past start time cannot be activated."); // F2
        Status = ScheduleStatus.Active;
        return new None();
    }

    
    public Result<None> RemoveCourt(CourtName name, DateTime removalMoment)
    {
        if (Status == ScheduleStatus.Deleted)
            return new ResultError("schedule.deleted", "Deleted schedules cannot be updated.");
        var removalDate = DateOnly.FromDateTime(removalMoment);
        if (Date < removalDate)
            return new ResultError("schedule.past", "Past daily schedules cannot be modified.");                // F1

        var court = _courts.FirstOrDefault(c => c.Name == name);
        if (court is null)
            return new ResultError("court.notFound", "No padel court with that name was found in the schedule."); // F2

        var removalTime = TimeOnly.FromDateTime(removalMoment);
        var sameDay = Date == removalDate;

        if (Status == ScheduleStatus.Active && sameDay && court.Bookings.Any(b => b.Time.Start >= removalTime))
            return new ResultError("court.hasLaterBookings",
                "Courts with bookings later on the same day cannot be removed.");                                // F4

        var affected = court.Bookings
            .Where(b => Date > removalDate || (sameDay && b.Time.Start > removalTime))                           // still upcoming
            .Select(b => b.Player)
            .Distinct()
            .ToList();

        _courts.Remove(court);

        if (Status == ScheduleStatus.Active && affected.Count > 0)
            Raise(new CourtRemovedBookingsCancelled(Id, name, affected));                                        // S2/S6 (S5: none)
        return new None();
    }

    
    public Result<None> Delete(DateOnly today)
    {
        if (Status == ScheduleStatus.Deleted)
            return new ResultError("schedule.alreadyDeleted", "The schedule is already deleted.");               // F3
        if (Date < today)
            return new ResultError("schedule.past", "A schedule in the past cannot be deleted.");                // F2
        if (Status == ScheduleStatus.Active && Date == today)
            return new ResultError("schedule.deleteTooLate", "A schedule cannot be deleted on the same day it runs."); // F4

        var wasActive = Status == ScheduleStatus.Active;
        var affected = _courts.SelectMany(c => c.Bookings).Select(b => b.Player).Distinct().ToList();

        Status = ScheduleStatus.Deleted;
        _courts.Clear();                                                                                         // all courts removed

        if (wasActive && affected.Count > 0)
            Raise(new ScheduleDeleted(Id, affected));                                                            // S1 notify
        return new None();
    }

    
    public Result<None> AddVipTimeSpan(TimeInterval span)
    {
        if (Status != ScheduleStatus.Draft)
            return new ResultError("schedule.notDraft", "VIP time spans can only be added while in draft.");    // F6
        if (!Time.Contains(span))
            return new ResultError("vip.outOfBounds", "The VIP time span must be within the schedule's time span."); // F3
        if (span.DurationMinutes() < 30)
            return new ResultError("vip.tooShort", "A VIP time span must be 30 minutes or more.");              // F5
        // F2 (overlapping non-VIP bookings) cannot occur in draft — there are no bookings yet.
        MergeVip(span);                                                                                          // S2/S3/S4
        return new None();
    }

    private void MergeVip(TimeInterval span)
    {
        var all = _vipSpans.Append(span).OrderBy(t => t.Start).ToList();
        var merged = new List<TimeInterval>();
        var curStart = all[0].Start;
        var curEnd = all[0].End;
        foreach (var t in all.Skip(1))
        {
            if (t.Start <= curEnd)                                  // overlapping or bordering -> extend
            {
                if (t.End > curEnd) curEnd = t.End;
            }
            else
            {
                merged.Add(TimeInterval.Trusted(curStart, curEnd));
                curStart = t.Start;
                curEnd = t.End;
            }
        }
        merged.Add(TimeInterval.Trusted(curStart, curEnd));
        _vipSpans.Clear();
        _vipSpans.AddRange(merged);
    }

    /// <summary>UC6 / UC14. Minute format (F9) is guarded by TimeInterval before we get here.
    /// F5-F8 (outside schedule bounds) collapse into the single Contains check.</summary>
    public Result<None> Book(Player player, CourtName courtName, TimeInterval time, DateTime now)
    {
        if (Status != ScheduleStatus.Active)
            return new ResultError("schedule.notActive", "Courts cannot be booked unless the schedule is active."); // F1/F2
        if (player.IsBlacklisted)
            return new ResultError("player.blacklisted", "Blacklisted players cannot book courts.");            // F14
        if (player.IsQuarantinedOn(Date))
            return new ResultError("player.quarantined", "The player cannot book courts on a quarantined date.");// F13

        var court = _courts.FirstOrDefault(c => c.Name == courtName);
        if (court is null)
            return new ResultError("court.notFound", "The selected court was not found.");                       // F4

        if (!Time.Contains(time))
            return new ResultError("booking.outOfBounds", "The booking must lie within the schedule's time interval."); // F5-F8
        var duration = time.DurationMinutes();
        if (duration < 60)
            return new ResultError("booking.tooShort", "A booking must be one hour or longer.");                // F10
        if (duration > 180)
            return new ResultError("booking.tooLong", "A booking can be at most three hours.");                 // F12
        if (court.HasOverlap(time))
            return new ResultError("booking.overlap", "The court is not available in the selected time span.");  // F11
        if (court.LeavesGapUnderHour(time, Time))
            return new ResultError("booking.gap", "A booking may not leave a gap shorter than one hour.");       // F18
        if (PlayerHasBookingOnThisSchedule(player.Id))
            return new ResultError("booking.duplicatePerDay", "A player can have a maximum of one booking per day."); // F17
        if (Date.ToDateTime(time.Start) <= now)
            return new ResultError("booking.past", "A booking cannot start in the past.");                       // F19
        if (!player.IsVipOn(Date) && OverlapsVip(time))
            return new ResultError("booking.vipOnly", "Non-VIP players cannot book over the VIP time span.");    // F15 (UC14 lets VIPs through)

        var booking = Booking.Create(time, player.Id);
        court.Add(booking);
        Raise(new BookingPlaced(Id, courtName, booking.Id, player.Id));
        return new None();
    }

    /// <summary>UC7. Cannot cancel a past booking (F1) or one starting in under an hour (F2).</summary>
    public Result<None> CancelBooking(BookingId bookingId, DateTime now)
    {
        var court = _courts.FirstOrDefault(c => c.Find(bookingId) is not null);
        if (court is null)
            return new ResultError("booking.notFound", "No booking was found.");                                // F3
        var booking = court.Find(bookingId)!;
        var start = Date.ToDateTime(booking.Time.Start);

        if (start <= now)
            return new ResultError("booking.past", "Past bookings cannot be cancelled.");                       // F1
        if ((start - now).TotalMinutes < 60)
            return new ResultError("booking.cancelTooLate", "A booking cannot be cancelled less than an hour before it starts."); // F2

        court.Remove(booking);
        Raise(new BookingCancelled(Id, court.Name, bookingId, NotificationQueue.PlayersToNotify()));            // S2 notify queue
        return new None();
    }

    /// <summary>UC16.</summary>
    public Result<None> JoinQueue(Player player)
    {
        NotificationQueue.Add(player.Id);
        return new None();
    }

    private bool PlayerHasBookingOnThisSchedule(PlayerId player)
        => _courts.SelectMany(c => c.Bookings).Any(b => b.Player == player);

    private bool OverlapsVip(TimeInterval time) => _vipSpans.Any(v => v.Overlaps(time));
}
