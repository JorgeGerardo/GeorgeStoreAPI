namespace GeorgeStore.Common.Core.Interfaces;

public interface IQueryHandler<TQuery, TResult>
{
    Task<TResult> Handle(TQuery query);
}