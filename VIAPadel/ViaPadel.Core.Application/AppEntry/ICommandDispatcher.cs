using ViaPadel.Core.Tools.OperationResult;

namespace ViaPadel.Core.Application.AppEntry;

public interface ICommandDispatcher
{
    public Task<Result> DispatchAsync<TCommand>(TCommand command);
}