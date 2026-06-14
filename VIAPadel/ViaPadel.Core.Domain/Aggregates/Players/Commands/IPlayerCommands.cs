using ViaPadel.Core.Domain;
using ViaPadel.Core.Domain.Aggregates.Players;

namespace ViaPadel.Core.Domain.Contracts;

public interface IPlayerCommands
{
    Task<Player?> GetByIdAsync(Guid id);
    Task<Player?> GetByEmailAsync(string email);
    Task<bool> ExistsByEmailAsync(string email);
    Task AddAsync(Player player);
    Task UpdateAsync(Player player);
}
