using ViaPadel.Core.Application.AppEntry;
using ViaPadel.Core.Domain.Aggregates.DailySchedules;
using ViaPadel.Core.Domain.Contracts;
using ViaPadel.Core.Tools.OperationResult;

namespace ViaPadel.Core.Application.Features.DailySchedule;

// UC4 - The manager activates the daily schedule.
public sealed record ActivateScheduleCommand(Guid ScheduleId);

public sealed class ActivateScheduleHandler : ICommandHandler<ActivateScheduleCommand>
{
    private readonly IDailyScheduleCommands _schedules;
    private readonly IDateTimeProvider _clock;

    public ActivateScheduleHandler(IDailyScheduleCommands schedules, IDateTimeProvider clock)
    {
        _schedules = schedules;
        _clock = clock;
    }

    public async Task<Result> HandleASyncCommand(ActivateScheduleCommand c)
    {
        var schedule = await _schedules.GetByIdAsync(c.ScheduleId);
        if (schedule is null) return ResultFactory.Error(Errors.Schedule.NotFound);              // F4
        if (schedule.Status == ScheduleStatus.Deleted) return ResultFactory.Error(Errors.Schedule.CannotActivateDeleted); // F3
        if (schedule.Status == ScheduleStatus.Active) return ResultFactory.Error(Errors.Schedule.AlreadyActive);          // F6
        if (schedule.Courts.Count == 0) return ResultFactory.Error(Errors.Schedule.NoCourts);    // F1

        var startsAt = schedule.Date.ToDateTime(schedule.Time.Start);
        if (startsAt <= _clock.Now) return ResultFactory.Error(Errors.Schedule.CannotActivatePast); // F2

        var conflicts = await _schedules.GetActiveByDateAsync(schedule.Date);
        if (conflicts.Any(s => s.Id != schedule.Id))
            return ResultFactory.Error(Errors.Schedule.DateConflict);                            // F5

        schedule.Activate(DateTime.Now);                                                                     // S1
        await _schedules.UpdateAsync(schedule);
        return ResultFactory.Ok();
    }
}
