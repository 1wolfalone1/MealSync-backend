namespace MealSync.Application.Shared;

public class Result
{
    protected internal Result(bool isSuccess, bool isWarning, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException();
        }

        if (!isSuccess && !isWarning && error == Error.None)
        {
            throw new InvalidOperationException();
        }

        IsWarning = isWarning;
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsWarning { get; }

    public bool IsSuccess { get; }

    public bool IsFailure => !(IsSuccess || IsWarning);

    public Error Error { get; }

    public static Result Success() => new(true, false, Error.None);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, false,Error.None);

    public static Result Warning() => new(false, true, Error.None);

    public static Result<TValue> Warning<TValue>(TValue value) => new(value, false, true, Error.None);

    public static Result Failure(Error error) => new(false, false, error);

    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, false, error);

    public static Result<TValue> Failure<TValue>(TValue value, Error error) => new(value, false, false, error);

    public static Result<TValue> Create<TValue>(TValue? value) => value is not null ? Success(value) : Failure<TValue>(Error.NullValue);
}
