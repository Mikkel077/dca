using Microsoft.EntityFrameworkCore;
using ViaPadel.Core.Domain.Aggregates.Players;
using ViaPadel.Core.Domain.Aggregates.Players.Contracts;

namespace ViaPadel.Infrastructure.SqliteDmPersistence;

public class PlayersRepository(SqliteDmContext context) : IPlayersRepository
{
    public Task AddAsync(Player player)
    {
        context.Players.Add(player);
        return Task.CompletedTask;
    }

    public async Task<Player> GetASync(Guid id)
    {
        var player = await context.Players.FirstOrDefaultAsync(x => x.Id.Value == id);
        if (player != null) return player;
        return null;
    }

    public Task RemoveAsync(Guid id)
    {
        context.Remove(context.Players.FirstOrDefault(x => x.Id.Value == id));
        return Task.CompletedTask;
    }
}