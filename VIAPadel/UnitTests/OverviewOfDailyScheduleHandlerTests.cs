using ViaPadel.Core.QueryContracts;
using ViaPadel.Infrastructure.EfcQueries;

namespace UnitTests;

public class OverviewOfDailyScheduleHandlerTests
{
    // NOTE: the handler is currently a stub. It never touches the injected
    // SqliteDmContext and never reads query.Date — it returns hardcoded data.
    // These tests pin that behavior. The ones labelled "characterization"
    // document things that look like bugs (see comments on each).

    private static OverviewOfDailyScheduleHandler CreateHandler()
        => new(context: null!); // safe: the stub never dereferences the context

    private static async Task<OverviewOfDailySchedule.Answer> HandleAsync(DateOnly date)
    {
        var handler = CreateHandler();
        return await handler.HandleAsync<object>(new OverviewOfDailySchedule.Query(date));
    }

    private static readonly DateOnly AnyDate = new(2025, 4, 9);

    [Fact]
    public async Task HandleAsync_ReturnsNonNullAnswer()
    {
        var answer = await HandleAsync(AnyDate);
        Assert.NotNull(answer);
    }

    // ---------------------------------------------------------------------
    // Time slots
    // ---------------------------------------------------------------------

    [Fact]
    public async Task HandleAsync_ReturnsEightTimeSlots()
    {
        var answer = await HandleAsync(AnyDate);
        Assert.Equal(8, answer.TimeSlots.Count);
    }

    [Fact]
    public async Task HandleAsync_ReturnsExpectedTimeSlots()
    {
        var answer = await HandleAsync(AnyDate);

        // Records have value equality, so the lists compare element-by-element.
        var expected = new List<OverviewOfDailySchedule.TimeSlotInfo>
        {
            new(new(15, 0), new(15, 30), false),
            new(new(15, 30), new(16, 0), false),
            new(new(16, 0), new(16, 30), false),
            new(new(16, 30), new(17, 0), false),
            new(new(17, 0), new(17, 30), true),
            new(new(17, 30), new(18, 0), true),
            new(new(18, 0), new(18, 30), true),
            new(new(18, 30), new(19, 0), true),
        };

        Assert.Equal(expected, answer.TimeSlots);
    }

    [Fact]
    public async Task HandleAsync_FirstFourSlots_AreNotVipOnly()
    {
        var answer = await HandleAsync(AnyDate);
        Assert.All(answer.TimeSlots.Take(4), slot => Assert.False(slot.IsVipOnly));
    }

    [Fact]
    public async Task HandleAsync_LastFourSlots_AreVipOnly()
    {
        var answer = await HandleAsync(AnyDate);
        Assert.All(answer.TimeSlots.Skip(4), slot => Assert.True(slot.IsVipOnly));
    }

    [Fact]
    public async Task HandleAsync_TimeSlots_AreContiguousHalfHourBlocks()
    {
        var answer = await HandleAsync(AnyDate);

        foreach (var slot in answer.TimeSlots)
            Assert.Equal(TimeSpan.FromMinutes(30), slot.EndTime - slot.StartTime);

        for (var i = 1; i < answer.TimeSlots.Count; i++)
            Assert.Equal(answer.TimeSlots[i - 1].EndTime, answer.TimeSlots[i].StartTime);
    }

    // ---------------------------------------------------------------------
    // Courts
    // ---------------------------------------------------------------------

    [Fact]
    public async Task HandleAsync_ReturnsTwoCourts()
    {
        var answer = await HandleAsync(AnyDate);
        Assert.Equal(2, answer.Courts.Count);
    }

    [Fact]
    public async Task HandleAsync_ReturnsExpectedCourtNamesAndDisplayNames()
    {
        var answer = await HandleAsync(AnyDate);

        Assert.Collection(answer.Courts,
            c =>
            {
                Assert.Equal("S1", c.CourtName);
                Assert.Equal("Single 1", c.DisplayName);
            },
            c =>
            {
                Assert.Equal("D4", c.CourtName);
                Assert.Equal("Double 4", c.DisplayName);
            });
    }

    [Fact]
    public async Task HandleAsync_CourtS1_HasExpectedBookings()
    {
        var answer = await HandleAsync(AnyDate);
        var s1 = answer.Courts.Single(c => c.CourtName == "S1");

        Assert.Collection(s1.Bookings,
            b =>
            {
                Assert.Equal(new TimeOnly(15, 0), b.StartTime);
                Assert.Equal(new TimeOnly(16, 0), b.EndTime);
                Assert.Equal("Troels", b.BookedByName);
            },
            b =>
            {
                Assert.Equal(new TimeOnly(17, 0), b.StartTime);
                Assert.Equal(new TimeOnly(18, 0), b.EndTime);
                Assert.Equal("Peter", b.BookedByName);
            });
    }

    [Fact]
    public async Task HandleAsync_CourtD4_HasExpectedBookings()
    {
        var answer = await HandleAsync(AnyDate);
        var d4 = answer.Courts.Single(c => c.CourtName == "D4");

        var booking = Assert.Single(d4.Bookings);
        Assert.Equal(new TimeOnly(16, 0), booking.StartTime);
        Assert.Equal(new TimeOnly(18, 0), booking.EndTime);
        Assert.Equal("Anna", booking.BookedByName);
    }

    // ---------------------------------------------------------------------
    // Cross-cutting invariants
    // ---------------------------------------------------------------------

    [Fact]
    public async Task HandleAsync_AllBookings_StartBeforeTheyEnd()
    {
        var answer = await HandleAsync(AnyDate);
        var bookings = answer.Courts.SelectMany(c => c.Bookings);
        Assert.All(bookings, b => Assert.True(b.StartTime < b.EndTime));
    }

    [Fact]
    public async Task HandleAsync_AllBookingIds_AreNonEmptyAndUnique()
    {
        var answer = await HandleAsync(AnyDate);
        var ids = answer.Courts.SelectMany(c => c.Bookings).Select(b => b.BookingId).ToList();

        Assert.DoesNotContain(Guid.Empty, ids);
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    // ---------------------------------------------------------------------
    // Characterization tests — these document current (likely unintended)
    // behavior. Update them when the handler is wired up to real data.
    // ---------------------------------------------------------------------

    // The handler ignores query.Date and always returns 2025-04-09.
    // When fixed, this should become: Assert.Equal(requested, answer.Date).
    [Theory]
    [InlineData(2025, 4, 9)]
    [InlineData(2030, 1, 1)]
    [InlineData(1999, 12, 31)]
    public async Task HandleAsync_IgnoresQueryDate_AlwaysReturnsHardcodedDate(int year, int month, int day)
    {
        var answer = await HandleAsync(new DateOnly(year, month, day));
        Assert.Equal(new DateOnly(2025, 4, 9), answer.Date);
    }
}