namespace GeorgeStore.Common.Shared;

public record QueryParams
{
    public int PageSize
    {
        get;
        init => field = value is < 0 or > 100 ? 10 : value;
    } = 10;

    public int Offset
    {
        get;
        init => field = value < 0 ? 0 : value;
    } = 0;
    public string? Term { get; init; }
}

