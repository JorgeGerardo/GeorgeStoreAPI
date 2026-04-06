namespace GeorgeStore.Core;

public class Error
{
    public string Message { get; }
    public int Code { get; }
    public Error(string Message, int Code)
    {
        this.Message = Message;
        this.Code = Code;
    }


    public static readonly Error None = new Error(string.Empty, default);
}
