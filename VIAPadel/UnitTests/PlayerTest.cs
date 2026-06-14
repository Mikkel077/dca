using ViaPadel.Core.Domain.Aggregates.Players;
using ViaPadel.Core.Tools.OperationResult;

namespace UnitTests;

public class PlayerTests
{
    // ───────────────────────────────────────────────────────────────────────
    //  TEST HARNESS ASSUMPTIONS — adjust these to match your codebase.
    //  Everything the tests below need from the supporting types funnels
    //  through this region, so there's only one place to fix if anything
    //  is named differently.
    // ───────────────────────────────────────────────────────────────────────

    // (1) Inputs that pass ViaEmail / PersonName / ProfilePicture validation.
    private const string ValidEmail = "alice@via.dk";
    private const string ValidFirst = "Alice";
    private const string ValidLast  = "Andersen";
    private const string ValidPic   = "https://cdn.via.dk/players/alice.png";

    private static readonly DateOnly Today = new(2026, 6, 15);

    // (2) Pull the value out of a successful Result<T>.
    //     Assumes a concrete Success<T> with a .Value property.
    private static T Value<T>(Result<T> result) =>
        Assert.IsType<Success<T>>(result).Value;

    // (3) Assert a Result is a success.
    private static void AssertSuccess<T>(Result<T> result) =>
        Assert.IsType<Success<T>>(result);

    // (4) Assert a Result is a failure and return the first error code.
    //     Assumes Failure<T>.Errors (IEnumerable<ResultError>) and ResultError.Code.
    private static string FailureCode<T>(Result<T> result) =>
        Assert.IsType<Failure<T>>(result).Errors.First().code;

    // (5) Read raised domain events off the aggregate.
    //     Assumes AggregateRoot exposes a DomainEvents enumerable.
    private static IReadOnlyList<object> Events(Player p) =>
        p.DomainEvents.Cast<object>().ToList();

    private static TEvent SingleEvent<TEvent>(Player p) =>
        Assert.Single(Events(p).OfType<TEvent>());

    // A freshly registered, clean player for use as a starting point.
    private static Player NewPlayer() =>
        Value(Player.Register(ValidEmail, ValidFirst, ValidLast, ValidPic));

    // ───────────────────────────────────────────────────────────────────────
    //  Register  (UC: registration)
    // ───────────────────────────────────────────────────────────────────────

    [Fact]
    public void Register_WithValidInput_ProducesCleanPlayer()
    {
        var result = Player.Register(ValidEmail, ValidFirst, ValidLast, ValidPic);

        var player = Value(result);
        Assert.NotEqual(default, player.Id);
        Assert.False(player.IsBlacklisted);
        Assert.Null(player.CurrentQuarantine);
        Assert.Null(player.Vip);
        Assert.False(player.IsQuarantinedOn(Today));
        Assert.False(player.IsVipOn(Today));
    }

    [Fact]
    public void Register_OnSuccess_RaisesPlayerRegistered()
    {
        var player = NewPlayer();

        var evt = SingleEvent<PlayerRegistered>(player);
        Assert.NotNull(evt);
    }

    [Fact]
    public void Register_WithInvalidEmail_Fails()
    {
        var result = Player.Register("not-an-email", ValidFirst, ValidLast, ValidPic);

        Assert.IsType<Failure<Player>>(result);
    }

    [Fact]
    public void Register_AggregatesAllValidationErrors()
    {
        // All three value objects invalid -> errors are collected, not short-circuited.
        var result = Player.Register(null, null, null, null);

        var failure = Assert.IsType<Failure<Player>>(result);
        Assert.True(failure.Errors.Count() > 1,
            "Expected validation errors from more than one value object to be aggregated.");
    }

    // ───────────────────────────────────────────────────────────────────────
    //  ApplyQuarantine  (UC9)
    // ───────────────────────────────────────────────────────────────────────

    [Fact]
    public void ApplyQuarantine_WhenNotQuarantined_StartsNewQuarantine() // S1
    {
        var player = NewPlayer();

        AssertSuccess(player.ApplyQuarantine(Today));

        Assert.NotNull(player.CurrentQuarantine);
        Assert.True(player.IsQuarantinedOn(Today));
        Assert.NotNull(SingleEvent<PlayerQuarantined>(player));
    }

    [Fact]
    public void ApplyQuarantine_WhenAlreadyActive_ExtendsByThreeDays() // S2
    {
        var player = NewPlayer();
        player.ApplyQuarantine(Today);
        var endBefore = player.CurrentQuarantine!.EndDate;

        AssertSuccess(player.ApplyQuarantine(Today)); // re-applied while still active

        Assert.Equal(endBefore.AddDays(3), player.CurrentQuarantine!.EndDate);
    }

    [Fact]
    public void ApplyQuarantine_WhenPreviousExpired_StartsFreshRatherThanExtends()
    {
        var player = NewPlayer();
        player.ApplyQuarantine(Today);
        var firstEnd = player.CurrentQuarantine!.EndDate;
        var baseLength = firstEnd.DayNumber - Today.DayNumber;

        var laterDay = firstEnd.AddDays(10); // well after the first quarantine lapsed
        AssertSuccess(player.ApplyQuarantine(laterDay));

        // A fresh start ends at laterDay + baseLength, not firstEnd + 3.
        Assert.Equal(laterDay.AddDays(baseLength), player.CurrentQuarantine!.EndDate);
        Assert.True(player.IsQuarantinedOn(laterDay));
    }

    [Fact]
    public void ApplyQuarantine_WhenBlacklisted_Fails() // F2
    {
        var player = NewPlayer();
        player.Blacklist();

        var result = player.ApplyQuarantine(Today);

        Assert.Equal("player.blacklisted", FailureCode(result));
        Assert.Null(player.CurrentQuarantine);
    }

    // ───────────────────────────────────────────────────────────────────────
    //  Blacklist  (UC10)
    // ───────────────────────────────────────────────────────────────────────

    [Fact]
    public void Blacklist_WhenNotBlacklisted_SetsFlagAndRaisesEvent()
    {
        var player = NewPlayer();

        AssertSuccess(player.Blacklist());

        Assert.True(player.IsBlacklisted);
        Assert.NotNull(SingleEvent<PlayerBlacklisted>(player));
    }

    [Fact]
    public void Blacklist_ClearsActiveQuarantineAndVip() // S2 / S5
    {
        var player = NewPlayer();
        player.ApplyQuarantine(Today);
        // grant VIP on a day the quarantine is no longer active so the grant is allowed
        var afterQuarantine = player.CurrentQuarantine!.EndDate.AddDays(10);
        player.GrantVip(afterQuarantine);
        Assert.NotNull(player.CurrentQuarantine);
        Assert.NotNull(player.Vip);

        AssertSuccess(player.Blacklist());

        Assert.True(player.IsBlacklisted);
        Assert.Null(player.CurrentQuarantine);
        Assert.Null(player.Vip);
    }

    [Fact]
    public void Blacklist_WhenAlreadyBlacklisted_Fails() // F2
    {
        var player = NewPlayer();
        player.Blacklist();

        var result = player.Blacklist();

        Assert.Equal("player.alreadyBlacklisted", FailureCode(result));
    }

    // ───────────────────────────────────────────────────────────────────────
    //  LiftBlacklist  (UC11)
    // ───────────────────────────────────────────────────────────────────────

    [Fact]
    public void LiftBlacklist_WhenBlacklisted_ClearsFlagAndRaisesEvent()
    {
        var player = NewPlayer();
        player.Blacklist();

        AssertSuccess(player.LiftBlacklist());

        Assert.False(player.IsBlacklisted);
        Assert.NotNull(SingleEvent<PlayerBlacklistLifted>(player));
    }

    [Fact]
    public void LiftBlacklist_WhenNotBlacklisted_Fails() // F2
    {
        var player = NewPlayer();

        var result = player.LiftBlacklist();

        Assert.Equal("player.notBlacklisted", FailureCode(result));
    }

    // ───────────────────────────────────────────────────────────────────────
    //  GrantVip  (UC12)
    // ───────────────────────────────────────────────────────────────────────

    [Fact]
    public void GrantVip_WhenEligible_GrantsVip() // S1 / S2
    {
        var player = NewPlayer();

        AssertSuccess(player.GrantVip(Today));

        Assert.NotNull(player.Vip);
        Assert.True(player.IsVipOn(Today));
        Assert.NotNull(SingleEvent<PlayerGrantedVip>(player));
    }

    [Fact]
    public void GrantVip_WhenAlreadyActive_ExtendsByThirtyDays() // S3
    {
        var player = NewPlayer();
        player.GrantVip(Today);
        var endBefore = player.Vip!.EndDate;

        AssertSuccess(player.GrantVip(Today)); // re-granted while still active

        Assert.Equal(endBefore.AddDays(30), player.Vip!.EndDate);
    }

    [Fact]
    public void GrantVip_WhenBlacklisted_Fails() // F2
    {
        var player = NewPlayer();
        player.Blacklist();

        var result = player.GrantVip(Today);

        Assert.Equal("player.blacklisted", FailureCode(result));
        Assert.Null(player.Vip);
    }

    [Fact]
    public void GrantVip_WhenQuarantineActive_Fails() // F3
    {
        var player = NewPlayer();
        player.ApplyQuarantine(Today);

        var result = player.GrantVip(Today);

        Assert.Equal("player.quarantined", FailureCode(result));
        Assert.Null(player.Vip);
    }

    [Fact]
    public void GrantVip_WhenQuarantineExpired_Succeeds()
    {
        var player = NewPlayer();
        player.ApplyQuarantine(Today);
        var afterQuarantine = player.CurrentQuarantine!.EndDate.AddDays(10);

        AssertSuccess(player.GrantVip(afterQuarantine));

        Assert.True(player.IsVipOn(afterQuarantine));
    }

    // ───────────────────────────────────────────────────────────────────────
    //  Query helpers
    // ───────────────────────────────────────────────────────────────────────

    [Fact]
    public void IsQuarantinedOn_IsFalse_ForCleanPlayer() =>
        Assert.False(NewPlayer().IsQuarantinedOn(Today));

    [Fact]
    public void IsVipOn_IsFalse_ForCleanPlayer() =>
        Assert.False(NewPlayer().IsVipOn(Today));
}