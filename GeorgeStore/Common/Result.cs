namespace GeorgeStore.Common;


public class Result(bool IsSuccess, Error Error)
{
    public bool IsSuccess { get; } = IsSuccess;
    public Error Error { get; } = Error;

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error)
    {
        if (error == Error.None)
            throw new ArgumentException("Cannot use Error.None for failure.");

        return new(false, error);
    }

    public static Result<T> Success<T>(T value) =>
        new(true, Error.None, value);

    public static Result<T> Failure<T>(Error error)
    {
        if (error == Error.None)
            throw new ArgumentException("Cannot use Error.None for failure.");

        return new(false, error, default);
    }
}

public partial class Result<T> : Result
{
    private readonly T? _value;
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("No value for failure.");

    public Result(bool IsSuccessfuly, Error Error, T? Value) : base(IsSuccessfuly, Error)
    {
        _value = Value;
    }

}

