using ViaPadel.Core.Domain.Aggregates.Players;

namespace ViaPadel.Core.Domain.Aggregates.DailySchedules;

/// <summary>UC16: players waiting to be notified if a booking on this schedule is cancelled.</summary>
public sealed class Queue : Entity<Guid>
{
    private readonly List<PlayerId> _waiting = new();

    private Queue() : base(Guid.NewGuid()) { }
    internal static Queue Empty() => new();

    public void Add(PlayerId playerId)
    {
        if (!_waiting.Contains(playerId)) _waiting.Add(playerId);
    }

    public IReadOnlyList<PlayerId> PlayersToNotify() => _waiting.AsReadOnly();
}
