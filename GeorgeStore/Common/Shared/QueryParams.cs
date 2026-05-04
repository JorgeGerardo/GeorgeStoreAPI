namespace GeorgeStore.Common.Shared;

public record QueryParams(int PageSize = 10, int Offset = 0, string? Term = null);

