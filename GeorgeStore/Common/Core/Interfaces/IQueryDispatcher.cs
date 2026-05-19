namespace GeorgeStore.Common.Core.Interfaces;

public interface IQueryDispatcher
{
    Task<TResult> Send<TQuery, TResult>(TQuery query);
}
