namespace GeorgeStore.Common.Shared;


public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

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

public partial class Result<T>(bool IsSuccess, Error Error, T? Value) : Result(IsSuccess, Error)
{

    private readonly T? _value = Value;
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("No value for failure.");
}

