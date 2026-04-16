namespace GeorgeStore.Common;

public record QueryParams(int PageSize = 10, int Offset = 0, string? Term = null);

