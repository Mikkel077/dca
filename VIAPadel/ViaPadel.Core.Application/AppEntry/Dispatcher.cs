using ViaPadel.Core.Tools.OperationResult;

namespace ViaPadel.Core.Application.AppEntry;

public class Dispatcher(IServiceProvider serviceProvider) : ICommandDispatcher
{
    public Task<Result> DispatchAsync<TCommand>(TCommand command)
    {
        Type serviceType = typeof(ICommandHandler<TCommand>);
        var service = serviceProvider.GetService(serviceType);
        if (service == null)
        {
            throw new ArgumentNullException(nameof(ICommandHandler<TCommand>));
        }
        
        ICommandHandler<TCommand> handler = (ICommandHandler<TCommand>) service;
        return handler.HandleASyncCommand(command);
    }
}