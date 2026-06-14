using Microsoft.Testing.Platform.Logging;
using ViaPadel.Core.Tools.OperationResult;

namespace ViaPadel.Core.Application.AppEntry;

public class LoggingDispatcher(ICommandDispatcher inner, ILogger<LoggingDispatcher> logger)
    : CommandDispatcherDecorator(inner)
{
    public override Task<Result> DispatchAsync<TCommand>(TCommand command)
    {
        Result dummy = new Success<None>(new None());
        return Task.FromResult(dummy);
    }
}