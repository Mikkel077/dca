using System.Windows.Input;
using ViaPadel.Core.Tools.OperationResult;

namespace ViaPadel.Core.Application.AppEntry;

public interface ICommandHandler<TCommand>
{
    Task<Result> HandleASyncCommand(TCommand command);
}