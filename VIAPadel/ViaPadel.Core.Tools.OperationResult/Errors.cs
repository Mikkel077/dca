namespace ViaPadel.Core.Tools.OperationResult;

/// <summary>
/// Central catalogue of <see cref="ResultError"/>s, grouped by area.
/// Codes are stable identifiers; messages are user-facing.
/// Each entry notes the use-case / scenario it primarily serves.
/// </summary>
public static class Errors
{
    public static class Schedule
    {
        public static ResultError NotFound =>
            new("schedule.not_found", "No daily schedule was found for the given criteria.");           // UC2.F1, UC3.F6, UC4.F4, UC6.F3, UC7.F4, UC8.F3, UC13.F1, UC15.F1
        public static ResultError DateInPast =>
            new("schedule.date_in_past", "A daily schedule cannot be placed in the past.");               // UC2.F2
        public static ResultError EndBeforeStart =>
            new("schedule.end_before_start", "The end time must be after the start time.");               // UC2.F3
        public static ResultError IntervalTooShort =>
            new("schedule.interval_too_short", "The time interval must span 60 minutes or more.");         // UC2.F4
        public static ResultError ActiveCannotBeModified =>
            new("schedule.active_immutable", "An active daily schedule cannot be modified, only deleted."); // UC2.F5
        public static ResultError MinutesNotHalfOrWhole =>
            new("schedule.bad_minutes", "The minutes of the times must be half or whole hours (:00 or :30).");// UC2.F6
        public static ResultError InThePast =>
            new("schedule.in_past", "Past daily schedules cannot be modified.");                           // UC3.F1, UC8.F1, UC15.F2
        public static ResultError Deleted =>
            new("schedule.deleted", "A deleted daily schedule cannot be modified.");                       // UC3.F3, UC15.F3
        public static ResultError NoCourts =>
            new("schedule.no_courts", "A daily schedule without padel courts cannot be activated.");        // UC4.F1
        public static ResultError CannotActivatePast =>
            new("schedule.activate_past", "A daily schedule with a past start time cannot be activated.");   // UC4.F2
        public static ResultError CannotActivateDeleted =>
            new("schedule.activate_deleted", "A deleted daily schedule cannot be activated.");               // UC4.F3
        public static ResultError DateConflict =>
            new("schedule.date_conflict", "Another active daily schedule already exists on this date.");     // UC4.F5
        public static ResultError AlreadyActive =>
            new("schedule.already_active", "The daily schedule is already active.");                         // UC4.F6
        public static ResultError MustBeDraft =>
            new("schedule.must_be_draft", "The daily schedule must be in draft status for this operation."); // UC13.F6
        public static ResultError DeleteTooLate =>
            new("schedule.delete_too_late", "A schedule cannot be deleted on the same day it is executed."); // UC15.F4
    }

    public static class Court
    {
        public static ResultError InvalidStartingLetter =>
            new("court.bad_letter", "A court name must start with S or D.");                                // UC3.F2
        public static ResultError InvalidEndingNumber =>
            new("court.bad_number", "A court name must end with a number between 1 and 10.");               // UC3.F4
        public static ResultError InvalidLength =>
            new("court.bad_length", "A court name must be 2 or 3 characters long.");                        // UC3.F5
        public static ResultError AlreadyExists =>
            new("court.exists", "That court is already added to the daily schedule.");                      // UC3.F7
        public static ResultError NotFound =>
            new("court.not_found", "The padel court was not found on the daily schedule.");                 // UC6.F4, UC8.F2
        public static ResultError HasLaterBookings =>
            new("court.has_later_bookings", "Courts with bookings later on the same day cannot be removed.");// UC8.F4
    }

    public static class Player
    {
        public static ResultError NotFound =>
            new("player.not_found", "No player was found for the given criteria.");                         // UC6.F16, UC9.F1, UC10.F1, UC11.F1, UC12.F1
        public static ResultError BadEmailDomain =>
            new("player.bad_email_domain", "Only people with a VIA mail (@via.dk) can register.");           // UC5.F1
        public static ResultError BadEmailFormat =>
            new("player.bad_email_format", "The email is not in a valid format.");                          // UC5.F2
        public static ResultError EmptyEmail =>
            new("player.empty_email", "The email must not be empty.");                                      // UC5.F3
        public static ResultError InvalidImageUri =>
            new("player.bad_uri", "The profile picture URL has an incorrect format.");                      // UC5.F4
        public static ResultError InvalidFirstName =>
            new("player.bad_first_name", "First name must be 2-25 letters (a-z) with no symbols or spaces.");// UC5.F5
        public static ResultError InvalidLastName =>
            new("player.bad_last_name", "Last name must be 2-25 letters (a-z) with no symbols or spaces.");  // UC5.F6
        public static ResultError EmailTaken =>
            new("player.email_taken", "That email is already registered.");                                 // UC5.F7
        public static ResultError AlreadyBlacklisted =>
            new("player.already_blacklisted", "The selected player is already blacklisted.");                // UC9.F2, UC10.F2
        public static ResultError NotBlacklisted =>
            new("player.not_blacklisted", "The selected player is not blacklisted.");                        // UC11.F2
        public static ResultError BlacklistedCannotBeVip =>
            new("player.blacklisted_no_vip", "Blacklisted players cannot be elevated to VIP.");              // UC12.F2
        public static ResultError QuarantinedCannotBeVip =>
            new("player.quarantined_no_vip", "Quarantined players cannot be elevated to VIP.");              // UC12.F3
    }

    public static class Booking
    {
        public static ResultError ScheduleNotActive =>
            new("booking.schedule_not_active", "Courts cannot be booked unless the daily schedule is active.");// UC6.F1, UC6.F2
        public static ResultError StartBeforeScheduleStart =>
            new("booking.start_before_schedule", "The booking start time is before the schedule start time.");// UC6.F5
        public static ResultError EndBeforeScheduleStart =>
            new("booking.end_before_schedule_start", "The booking end time is before the schedule start time.");// UC6.F6
        public static ResultError StartAfterScheduleEnd =>
            new("booking.start_after_schedule_end", "The booking start time is after the schedule end time.");// UC6.F7
        public static ResultError EndAfterScheduleEnd =>
            new("booking.end_after_schedule_end", "The booking end time is after the schedule end time.");    // UC6.F8
        public static ResultError BadMinutes =>
            new("booking.bad_minutes", "Booking times must be on the hour or half-hour (minutes :00 or :30).");// UC6.F9
        public static ResultError TooShort =>
            new("booking.too_short", "A booking must be one hour or longer.");                               // UC6.F10
        public static ResultError TooLong =>
            new("booking.too_long", "A booking must be three hours or shorter.");                            // UC6.F12
        public static ResultError Overlap =>
            new("booking.overlap", "The court is not available in the selected time span.");                 // UC6.F11
        public static ResultError Quarantined =>
            new("booking.quarantined", "You cannot book courts on dates where you are quarantined.");        // UC6.F13
        public static ResultError Blacklisted =>
            new("booking.blacklisted", "Blacklisted players cannot book courts.");                          // UC6.F14
        public static ResultError VipOnly =>
            new("booking.vip_only", "Non-VIP players cannot place bookings overlapping the VIP time span."); // UC6.F15
        public static ResultError OneBookingPerDay =>
            new("booking.one_per_day", "A player can have a maximum of one booking per day.");               // UC6.F17
        public static ResultError LeavesShortGap =>
            new("booking.short_gap", "A booking may not leave gaps shorter than one hour.");                 // UC6.F18
        public static ResultError StartsInThePast =>
            new("booking.starts_in_past", "A booking cannot start in the past.");                            // UC6.F19
        public static ResultError NotFound =>
            new("booking.not_found", "No matching booking was found.");                                      // UC7.F3
        public static ResultError InThePast =>
            new("booking.in_past", "Past bookings cannot be cancelled.");                                    // UC7.F1
        public static ResultError CancelTooLate =>
            new("booking.cancel_too_late", "Bookings cannot be cancelled less than one hour before start.");  // UC7.F2
    }

    public static class Vip
    {
        public static ResultError OverlapsNonVipBookings =>
            new("vip.overlaps_bookings", "The chosen VIP time span overlaps existing bookings by non-VIP players.");// UC13.F2
        public static ResultError OutsideSchedule =>
            new("vip.outside_schedule", "The VIP time span must be inside the daily schedule time span.");    // UC13.F3
        public static ResultError BadMinutes =>
            new("vip.bad_minutes", "VIP time spans must start/end on whole or half hours (:00 or :30).");     // UC13.F4
        public static ResultError TooShort =>
            new("vip.too_short", "A VIP time span must be 30 minutes or more.");                              // UC13.F5
    }
}
