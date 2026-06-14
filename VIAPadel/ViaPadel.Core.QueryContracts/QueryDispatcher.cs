namespace ViaPadel.Core.QueryContracts;

public class QueryDispatcher(IServiceProvider serviceProvider) : IQueryDispatcher
{
    public Task<TAnswer> DispatchAsync<TAnswer>(IQuery<TAnswer> query)
    {
        Type queryInterfaceWithTypes = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TAnswer));
        dynamic handler = serviceProvider.GetService(queryInterfaceWithTypes);

        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler));
        }
        
        return handler.HandleAsync((dynamic)query);
    }
}