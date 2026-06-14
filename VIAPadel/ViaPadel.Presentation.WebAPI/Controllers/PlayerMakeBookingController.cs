using Microsoft.AspNetCore.Mvc;
using ViaPadel.Core.Application.AppEntry;
using ViaPadel.Core.Application.Features.DailySchedule;
using ViaPadel.Core.Tools.OperationResult;

namespace ViaPadel.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PlayerMekBookingController : ControllerBase
{
    private readonly ICommandDispatcher _dispatcher;

    public PlayerMekBookingController(ICommandDispatcher dispatcher)
        => _dispatcher = dispatcher;

    [HttpPost]
    public async Task<IActionResult> MakeBooking([FromBody] PlayerMakesABookingRequest request)
    {
        var command = new PlayerMakesABookingCommand(
            request.PlayerId,
            request.CourtName,
            request.Date,
            request.StartTime,
            request.EndTime);

        var result = await _dispatcher.DispatchAsync(command);
        
        if (result.IsFailure)
        {
            return BadRequest(result.Errors);
        }

        return NoContent();
    }
}

public sealed record PlayerMakesABookingRequest(
    Guid PlayerId,
    string CourtName,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime);