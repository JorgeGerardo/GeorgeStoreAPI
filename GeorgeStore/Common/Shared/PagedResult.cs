namespace GeorgeStore.Common.Shared;

public record PagedResult<T>(IEnumerable<T> Items, int Total);
