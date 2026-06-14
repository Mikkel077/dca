using ViaPadel.Core.Tools.OperationResult;

namespace ViaPadel.Core.QueryContracts;

public interface IQueryHandler<TQuery, TAnswer>
{
    Task<TAnswer>HandleAsync<T>(TQuery query);
}