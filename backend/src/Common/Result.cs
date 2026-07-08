using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace UKPS.Api.Common;

[StructLayout(LayoutKind.Auto)]
[SuppressMessage(
    "Design",
    "CA1000:Do not declare static members on generic types",
    Justification = "Neverthrow-style construction API; type arguments are inferred at call sites via the target type."
)]
public readonly record struct Result<TValue, TError>
    where TValue : notnull
    where TError : notnull
{
    private readonly bool _isOk;

    // Distinguishes factory-created instances from default(Result<,>), which
    // holds neither a value nor an error and must not report IsOk or IsErr.
    private readonly bool _isInitialized;

    private Result(TValue? value, TError? error, bool isOk)
    {
        Value = value;
        Error = error;
        _isOk = isOk;
        _isInitialized = true;
    }

    public TValue? Value { get; }

    public TError? Error { get; }

    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsOk => _isInitialized && _isOk;

    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsErr => _isInitialized && !_isOk;

    public static Result<TValue, TError> Ok(TValue value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new Result<TValue, TError>(value, default, isOk: true);
    }

    public static Result<TValue, TError> Err(TError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new Result<TValue, TError>(default, error, isOk: false);
    }

    public TResult Match<TResult>(Func<TValue, TResult> onOk, Func<TError, TResult> onErr)
    {
        ArgumentNullException.ThrowIfNull(onOk);
        ArgumentNullException.ThrowIfNull(onErr);

        if (IsOk)
        {
            return onOk(Value);
        }

        if (IsErr)
        {
            return onErr(Error);
        }

        throw new InvalidOperationException(
            "Result was created with 'default' and holds neither a value nor an error."
        );
    }
}
