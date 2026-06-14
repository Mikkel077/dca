namespace ViaPadel.Core.Domain.Aggregates.Players.Contracts;

public interface IPlayersRepository
{
    Task AddAsync(Player player);
    Task<Player> GetASync(Guid id);
    Task RemoveAsync(Guid id);
}