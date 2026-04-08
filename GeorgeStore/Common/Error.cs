namespace GeorgeStore.Common;

public class Error(string Tittle, string Message, string Code)
{
    public string Tittle { get; } = Tittle;
    public string Message { get; } = Message;
    public string Code { get; } = Code;

    public static readonly Error None = new(string.Empty, string.Empty, string.Empty);
}

