using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace UKPS.Api.Application.Common;

/// <summary>
/// Represents the result of an operation that either succeeds with a value of
/// type <typeparamref name="TValue"/> or fails with an error of type
/// <typeparamref name="TError"/>.
/// </summary>
/// <typeparam name="TValue">The success value type.</typeparam>
/// <typeparam name="TError">The error value type.</typeparam>
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

    /// <summary>
    /// Gets the success value when <see cref="IsOk"/> is <see langword="true"/>.
    /// </summary>
    public TValue? Value { get; }

    /// <summary>
    /// Gets the error value when <see cref="IsErr"/> is <see langword="true"/>.
    /// </summary>
    public TError? Error { get; }

    /// <summary>
    /// Gets a value indicating whether this result represents a successful outcome.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsOk => _isInitialized && _isOk;

    /// <summary>
    /// Gets a value indicating whether this result represents a failed outcome.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsErr => _isInitialized && !_isOk;

    /// <summary>
    /// Creates a successful result containing the specified value.
    /// </summary>
    /// <param name="value">The success value.</param>
    /// <returns>A result in the successful state.</returns>
    public static Result<TValue, TError> Ok(TValue value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new Result<TValue, TError>(value, default, isOk: true);
    }

    /// <summary>
    /// Creates a failed result containing the specified error.
    /// </summary>
    /// <param name="error">The error value.</param>
    /// <returns>A result in the failed state.</returns>
    public static Result<TValue, TError> Err(TError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new Result<TValue, TError>(default, error, isOk: false);
    }

    /// <summary>
    /// Executes the delegate corresponding to the current result state.
    /// </summary>
    /// <typeparam name="TResult">The return type.</typeparam>
    /// <param name="onOk">Invoked when the result is successful.</param>
    /// <param name="onErr">Invoked when the result is a failure.</param>
    /// <returns>The value returned by the invoked delegate.</returns>
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
