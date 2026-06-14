namespace ViaPadel.Core.Domain.Aggregates.Players;

public sealed record PlayerRegistered(PlayerId PlayerId, Aggregates.Players.ViaEmail Email) : DomainEvent;
public sealed record PlayerQuarantined(PlayerId PlayerId, DateOnly Until) : DomainEvent;
public sealed record PlayerBlacklisted(PlayerId PlayerId) : DomainEvent;
public sealed record PlayerBlacklistLifted(PlayerId PlayerId) : DomainEvent;
public sealed record PlayerGrantedVip(PlayerId PlayerId, DateOnly Until) : DomainEvent;
