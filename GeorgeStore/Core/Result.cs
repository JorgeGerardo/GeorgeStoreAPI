namespace GeorgeStore.Core;


public class Result
{
    public bool IsSuccess { get; }
    public Error Error { get; set; }

    public Result(bool IsSuccess, Error Error)
    {
        this.Error = Error;
        this.IsSuccess = IsSuccess;
    }

    public static Result Success() => new Result(true, Error.None);
    public static Result Failure(Error error) => new Result(false, error);

}

public partial class Result<T> : Result
{
    public T? Value { get; }

    public Result(bool IsSuccessfuly, Error Error, T? Value) : base(IsSuccessfuly, Error)
    {
        this.Value = Value;
    }

    public static Result<T> Success(T Value) => new Result<T>(true, Error.None, Value);

    public static Result<T> Failure(Error Error, T value) => new(false, Error, value);

}

