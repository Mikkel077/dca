using ViaPadel.Core.Domain.Aggregates;

namespace ViaPadel.Core.Domain.Common;

public interface IGenericRepository<T> where T : AggregateRoot<T>
{
    Task<T> GetAsync(Guid id);
    Task RemoveAsync(Guid id);
    Task AddAsync(T aggregate);
}