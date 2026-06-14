using ViaPadel.Core.Tools.OperationResult;

namespace ViaPadel.Core.Domain.Common;

public interface IUnitOfWork
{
    Task<Result> SaveChangesASync();
}