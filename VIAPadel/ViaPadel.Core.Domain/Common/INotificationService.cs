using ViaPadel.Core.Domain.Aggregates.DailySchedules;

namespace ViaPadel.Core.Domain.Common;

/// <summary>
/// Email/notification side-effect, hidden behind a contract so it can be faked now
/// and swapped for real sending later (session 5 approach; could later be Domain Events).
/// </summary>
public interface INotificationService
{
    /// <summary>A player's booking was cancelled (court removed, quarantine, blacklist, schedule delete).</summary>
    Task NotifyBookingCancelledAsync(string playerEmail, DateOnly date, Booking booking);

    /// <summary>UC7.S2 / UC9.S4 / UC10.S4: players queued for a freed slot are told it opened up.</summary>
    Task NotifyQueuedPlayersAsync(Guid scheduleId, DateOnly date, string courtName, TimeOnly start, TimeOnly end);

    /// <summary>UC12.S2/S3: the player was granted/extended VIP.</summary>
    Task NotifyVipGrantedAsync(string playerEmail, DateOnly vipUntil);
}
