using GeorgeStore.Common.Core.Interfaces;

namespace GeorgeStore.Infrastructure.CQRS;

public class QueryDispatcher(IServiceProvider provider) : IQueryDispatcher
{
    public Task<TResult> Send<TQuery, TResult>(TQuery query)
    {
        var handler = provider.GetRequiredService<IQueryHandler<TQuery, TResult>>();
        return handler.Handle(query);
    }
}