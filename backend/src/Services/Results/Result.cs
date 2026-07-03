using System.Diagnostics.CodeAnalysis;

namespace UKPS.Api.Services.Results;

public class Result
{
    protected Result(bool isSuccess, ResultError? error)
    {
        if (isSuccess && error is not null)
        {
            throw new ArgumentException(
                "A successful result cannot contain an error.",
                nameof(error)
            );
        }

        if (!isSuccess && error is null)
        {
            throw new ArgumentException("A failed result must contain an error.", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }
    public ResultError? Error { get; }

    public static Result Success() => new(isSuccess: true, error: null);

    public static Result<T> Success<T>(T value) => new(value);

    public static Result Failure(ResultError error) => new(isSuccess: false, error: error);

    public static Result<T> Failure<T>(ResultError error) => new(error);
}
