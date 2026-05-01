namespace GeorgeStore.Common;

public sealed record Error(string Title, string Message, string Code, ErrorType Type)
{
    public static readonly Error None = new(string.Empty, string.Empty, string.Empty, ErrorType.None);
};

public enum ErrorType
{
    None,
    NotFound,
    Conflict,
    Validation,
    Unauthorized,
    Forbidden,
}