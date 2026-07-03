namespace UKPS.Api.Services.Results;

public sealed class Result<T> : Result
{
    internal Result(T value)
        : base(isSuccess: true, error: null)
    {
        Value = value;
    }

    internal Result(ResultError error)
        : base(isSuccess: false, error: error) { }

    public T? Value { get; }
}
