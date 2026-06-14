using Moq;
using ViaPadel.Core.Application.Features.DailySchedule;
using ViaPadel.Core.Domain.Aggregates.DailySchedules;
using ViaPadel.Core.Domain.Aggregates.Players;
using ViaPadel.Core.Domain.Contracts;
using ViaPadel.Core.Tools.OperationResult;
// TODO: adjust these to match your actual namespaces / project references.

namespace UnitTests;

public sealed class PlayerMakesABookingTests
{
    private readonly Mock<IDailyScheduleCommands> _schedules = new(MockBehavior.Strict);
    private readonly Mock<IPlayerCommands> _players = new(MockBehavior.Strict);
    private readonly Mock<IDateTimeProvider> _clock = new();

    private readonly PlayerMakesABooking _sut;

    public PlayerMakesABookingTests()
    {
        _sut = new PlayerMakesABooking(_schedules.Object, _players.Object, _clock.Object);
    }

    [Fact]
    public async Task ScheduleNotFound_ReturnsError_AndNeverTouchesPlayerOrSave()
    {
        var command = ValidCommand();
        _schedules
            .Setup(s => s.GetByDateAsync(command.Date))
            .ReturnsAsync((DailySchedule?)null); // returns null schedule

        var result = await _sut.HandleASyncCommand(command);

        AssertFailure(result);
        _players.Verify(p => p.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _schedules.Verify(s => s.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task PlayerNotFound_ReturnsError_AndDoesNotSave()
    {
        var command = ValidCommand();
        _schedules
            .Setup(s => s.GetByDateAsync(command.Date))
            .ReturnsAsync(BuildSchedule());
        _players
            .Setup(p => p.GetByIdAsync(command.PlayerId))
            .ReturnsAsync((Player?)null); // returns null player

        var result = await _sut.HandleASyncCommand(command);

        AssertFailure(result);
        _schedules.Verify(s => s.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task InvalidCourtName_ReturnsError_AndDoesNotSave()
    {
        // "" should fail CourtName.Create; the handler must bail before Book/Save.
        var command = ValidCommand() with { CourtName = "" };
        ArrangeScheduleAndPlayerFound(command);

        var result = await _sut.HandleASyncCommand(command);

        AssertFailure(result);
        _schedules.Verify(s => s.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task InvalidTimeInterval_ReturnsError_AndDoesNotSave()
    {
        // End before Start should fail TimeInterval.Create (confirm against your rule).
        var command = ValidCommand() with
        {
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(9, 0),
        };
        ArrangeScheduleAndPlayerFound(command);

        var result = await _sut.HandleASyncCommand(command);

        AssertFailure(result);
        _schedules.Verify(s => s.SaveChangesAsync(), Times.Never);
    }

    // ----------------------------------------------------------------------
    // Branches that depend on real domain behaviour of DailySchedule.Book.
    // These need a real aggregate arranged into the right state — see notes.
    // ----------------------------------------------------------------------

    [Fact]
    public async Task BookFails_ReturnsError_AndDoesNotSave()
    {
        var command = ValidCommand();
        var schedule = BuildScheduleWhereBookFails(command);

        _schedules.Setup(s => s.GetByDateAsync(command.Date)).ReturnsAsync(schedule);
        _players.Setup(p => p.GetByIdAsync(command.PlayerId)).ReturnsAsync(BuildPlayer());

        var result = await _sut.HandleASyncCommand(command);

        AssertFailure(result);
        _schedules.Verify(s => s.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task AllValid_BooksAndSaves_ReturnsOk()
    {
        var command = ValidCommand();
        // TODO: build a schedule whose Book(...) succeeds for this command.
        var schedule = BuildScheduleWhereBookSucceeds(command);

        _schedules.Setup(s => s.GetByDateAsync(command.Date)).ReturnsAsync(schedule);
        _players.Setup(p => p.GetByIdAsync(command.PlayerId)).ReturnsAsync(BuildPlayer());
        _schedules.Setup(s => s.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _sut.HandleASyncCommand(command);

        AssertSuccess(result);
        _schedules.Verify(s => s.SaveChangesAsync(), Times.Once);
    }

    // ----------------------------------------------------------------------
    // Helpers
    // ----------------------------------------------------------------------

    private static PlayerMakesABookingCommand ValidCommand() => new(
        PlayerId: Guid.NewGuid(),
        CourtName: "Center Court",
        Date: new DateOnly(2026, 6, 20),
        StartTime: new TimeOnly(10, 0),
        EndTime: new TimeOnly(11, 0));

    private void ArrangeScheduleAndPlayerFound(PlayerMakesABookingCommand command)
    {
        _schedules.Setup(s => s.GetByDateAsync(command.Date)).ReturnsAsync(BuildSchedule());
        _players.Setup(p => p.GetByIdAsync(command.PlayerId)).ReturnsAsync(BuildPlayer());
    }

    
    private static void AssertFailure(Result result) => Assert.False(result.IsSuccess);
    private static void AssertSuccess(Result result) => Assert.True(result.IsSuccess);
    
    private static DailySchedule BuildSchedule()
        => throw new NotImplementedException("Use the real DailySchedule factory.");

    private static Player BuildPlayer()
        => throw new NotImplementedException("Use the real Player factory.");

    private static DailySchedule BuildScheduleWhereBookFails(PlayerMakesABookingCommand command)
        => throw new NotImplementedException("Arrange a schedule whose Book(...) fails.");

    private static DailySchedule BuildScheduleWhereBookSucceeds(PlayerMakesABookingCommand command)
        => throw new NotImplementedException("Arrange a schedule whose Book(...) succeeds.");
}