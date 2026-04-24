namespace GeorgeStore.Common;

public record PagedResult<T>(IEnumerable<T> Items, int Total);
