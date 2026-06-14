using ViaPadel.Core.Tools.OperationResult;

namespace ViaPadel.Core.Application.AppEntry;

public abstract class CommandDispatcherDecorator(ICommandDispatcher inner) : ICommandDispatcher
{
    protected ICommandDispatcher Inner { get; } = inner;

    public virtual Task<Result> DispatchAsync<TCommand>(TCommand command)
        => Inner.DispatchAsync(command);
}