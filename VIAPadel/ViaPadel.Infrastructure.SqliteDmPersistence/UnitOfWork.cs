using Microsoft.EntityFrameworkCore;
using ViaPadel.Core.Domain.Common;
using ViaPadel.Core.Tools.OperationResult;

namespace ViaPadel.Infrastructure.SqliteDmPersistence;

public class UnitOfWork(SqliteDmContext context) : IUnitOfWork
{
    public async Task<Result> SaveChangesASync()
    {
        try
        {
            await context.SaveChangesAsync();
            return new Success<None>(new None());
        }
        catch (DbUpdateException ex)
        {
            return new Failure<None>(
                [new ResultError("Persistence.SaveFailed", ex.Message)]);
        }
    }
}