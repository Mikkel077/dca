using ViaPadel.Core.Tools.OperationResult;

namespace ViaPadel.Core.Domain.Aggregates.Players;

public sealed class Player : AggregateRoot<PlayerId>
{
    private Player() { }
    public ViaEmail Email { get; private set; }
    public PersonName Name { get; private set; }
    public ProfilePicture Picture { get; private set; }
    public bool IsBlacklisted { get; private set; }
    public Quarantine? CurrentQuarantine { get; private set; } // renamed from diagram "Quarantine" (clashed with the type)
    public VipStatus? Vip { get; private set; }

    private Player(PlayerId id, ViaEmail email, PersonName name, ProfilePicture picture) : base(id)
    {
        Email = email;
        Name = name;
        Picture = picture;
    }
    
    public static Result<Player> Register(string? email, string? firstName, string? lastName, string? pictureUri)
    {
        var errors = new List<ResultError>();
        ViaEmail.Create(email).TryValue(errors, out var emailVo);
        PersonName.Create(firstName, lastName).TryValue(errors, out var nameVo);
        ProfilePicture.Create(pictureUri).TryValue(errors, out var pictureVo);
        if (errors.Count > 0) return new Failure<Player>(errors);

        var player = new Player(PlayerId.New(), emailVo, nameVo, pictureVo);
        player.Raise(new PlayerRegistered(player.Id, emailVo));
        return player;
    }

    public Result<None> ApplyQuarantine(DateOnly today)
    {
        if (IsBlacklisted)
            return new ResultError("player.blacklisted", "Blacklisted players cannot be quarantined."); // UC9 F2
        CurrentQuarantine = CurrentQuarantine is not null && CurrentQuarantine.IsActiveOn(today)
            ? CurrentQuarantine.Extend(3)         // UC9 S2
            : Quarantine.StartingOn(today);       // UC9 S1
        Raise(new PlayerQuarantined(Id, CurrentQuarantine.EndDate)); // service cancels overlapping bookings (S3/S4)
        return new None();
    }

    public Result<None> Blacklist()
    {
        if (IsBlacklisted)
            return new ResultError("player.alreadyBlacklisted", "The player is already blacklisted."); // UC10 F2
        IsBlacklisted = true;
        CurrentQuarantine = null;                 // UC10 S2
        Vip = null;                               // UC10 S5
        Raise(new PlayerBlacklisted(Id));         // service cancels future bookings (S3/S4)
        return new None();
    }

    public Result<None> LiftBlacklist()
    {
        if (!IsBlacklisted)
            return new ResultError("player.notBlacklisted", "The player is not blacklisted."); // UC11 F2
        IsBlacklisted = false;
        Raise(new PlayerBlacklistLifted(Id));
        return new None();
    }

    public Result<None> GrantVip(DateOnly today)
    {
        if (IsBlacklisted)
            return new ResultError("player.blacklisted", "Blacklisted players cannot be elevated to VIP."); // UC12 F2
        if (CurrentQuarantine is not null && CurrentQuarantine.IsActiveOn(today))
            return new ResultError("player.quarantined", "Quarantined players cannot be elevated to VIP."); // UC12 F3
        Vip = Vip is not null && Vip.IsActiveOn(today)
            ? Vip.Extend(30)            // UC12 S3
            : VipStatus.GrantedOn(today); // UC12 S1
        Raise(new PlayerGrantedVip(Id, Vip.EndDate)); // notify (S2)
        return new None();
    }

    public bool IsQuarantinedOn(DateOnly date) => CurrentQuarantine?.IsActiveOn(date) ?? false;
    public bool IsVipOn(DateOnly date) => Vip?.IsActiveOn(date) ?? false;
}
