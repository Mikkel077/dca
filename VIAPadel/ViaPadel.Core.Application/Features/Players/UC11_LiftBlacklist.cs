using ViaPadel.Core.Application.AppEntry;
using ViaPadel.Core.Domain.Contracts;
using ViaPadel.Core.Tools.OperationResult;

namespace ViaPadel.Core.Application.Players;

// UC11 - The manager lifts the blacklisting of a player.
public sealed record LiftBlacklistCommand(Guid PlayerId);

public sealed class LiftBlacklistHandler : ICommandHandler<LiftBlacklistCommand>
{
    private readonly IPlayerCommands _players;

    public LiftBlacklistHandler(IPlayerCommands players) => _players = players;

    public async Task<Result> HandleASyncCommand(LiftBlacklistCommand c)
    {
        var player = await _players.GetByIdAsync(c.PlayerId);
        if (player is null) return ResultFactory.Error(Errors.Player.NotFound);          // F1
        if (!player.IsBlacklisted) return ResultFactory.Error(Errors.Player.NotBlacklisted); // F2

        player.LiftBlacklist();  // S1
        await _players.UpdateAsync(player);
        return ResultFactory.Ok();
    }
}
