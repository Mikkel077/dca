using ViaPadel.Core.Application.AppEntry;
using ViaPadel.Core.Domain.Aggregates.DailySchedules;
using ViaPadel.Core.Domain.Common;
using ViaPadel.Core.Domain.Contracts;
using ViaPadel.Core.Tools.OperationResult;

namespace ViaPadel.Core.Application.Features.DailySchedule;

public sealed record PlayerMakesABookingCommand(
    Guid PlayerId,
    string CourtName,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime); 

public class PlayerMakesABooking : ICommandHandler<PlayerMakesABookingCommand>
{
    private readonly IDailyScheduleCommands _schedules;
    private readonly IDateTimeProvider _clock;
    private readonly IPlayerCommands _players;
    private readonly IUnitOfWork _unitOfWork;
    
    public PlayerMakesABooking(
        IDailyScheduleCommands schedules,
        IPlayerCommands players,
        IDateTimeProvider clock)
    {
        _schedules = schedules;
        _players = players;
        _clock = clock;
    }
    
    
    public async Task<Result> HandleASyncCommand(PlayerMakesABookingCommand command)
    {
        var schedule = await _schedules.GetByDateAsync(command.Date);
        if (schedule == null)
        {
            return ResultFactory.Error(Errors.Schedule.NotFound);
        }

        var player = await _players.GetByIdAsync(command.PlayerId);
        if (player == null)
        {
            return ResultFactory.Error(Errors.Player.NotFound);
        }

        var errors = new List<ResultError>();

        if (!CourtName.Create(command.CourtName).TryValue(errors, out var courtName) ||
            !TimeInterval.Create(command.StartTime, command.EndTime).TryValue(errors, out var time))
        {
            return ResultFactory.Error();
        }

        if (!schedule.Book(player, courtName, time, _clock.Now).TryValue(errors, out _))
        {
            return ResultFactory.Error();
        }

        await _schedules.AddAsync(schedule);
        await _unitOfWork.SaveChangesASync();
        
        return ResultFactory.Ok();
    }
}