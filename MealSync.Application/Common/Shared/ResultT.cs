namespace MealSync.Application.Shared;

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, bool isWarning, Error error)
        : base(isSuccess, isWarning, error)
    {
        _value = value;
    }

    public TValue Value => _value;

    public static implicit operator Result<TValue>(TValue? value)
    {
        return Create(value);
    }
}