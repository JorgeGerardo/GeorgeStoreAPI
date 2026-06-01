namespace GeorgeStore.Common.Shared;

public record QueryParams
{
    public int PageSize
    {
        get;
        init => field = value is < 0 or > 100 ? 10 : value;
    }

    public int Offset
    {
        get;
        init => field = value < 0 ? 0 : value;
    }
    public string? Term { get; init; }
}

