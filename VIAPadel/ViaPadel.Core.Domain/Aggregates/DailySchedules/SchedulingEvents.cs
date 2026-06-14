using ViaPadel.Core.Domain.Aggregates.Players;

namespace ViaPadel.Core.Domain.Aggregates.DailySchedules;

public sealed record BookingPlaced(ScheduleId Schedule, CourtName Court, BookingId Booking, PlayerId Player) : DomainEvent;
public sealed record BookingCancelled(ScheduleId Schedule, CourtName Court, BookingId Booking,
    IReadOnlyList<PlayerId> QueuedToNotify) : DomainEvent;                                  // UC7 S2
public sealed record CourtRemovedBookingsCancelled(ScheduleId Schedule, CourtName Court,
    IReadOnlyList<PlayerId> AffectedPlayers) : DomainEvent;                                 // UC8 S2/S6
public sealed record ScheduleDeleted(ScheduleId Schedule, IReadOnlyList<PlayerId> AffectedPlayers) : DomainEvent; // UC15 S1
