using System.Diagnostics.CodeAnalysis;

namespace UKPS.Api.Application.Common;

/// <summary>
/// Represents the outcome of an operation that either succeeds or fails with an
/// error of type <typeparamref name="TError"/>.
/// </summary>
/// <typeparam name="TError">The type of the error returned when the operation fails.</typeparam>
public interface IResult<out TError>
{
    /// <summary>
    /// Gets the error associated with a failed operation.
    /// </summary>
    /// <remarks>
    /// This property is <see langword="null"/> when the operation succeeds.
    /// When <see cref="IsErr"/> is <see langword="true"/>, this property is
    /// guaranteed to be non-<see langword="null"/>.
    /// </remarks>
    [MemberNotNullWhen(true, nameof(Error))]
    TError? Error { get; }

    /// <summary>
    /// Gets a value indicating whether the operation completed successfully.
    /// </summary>
    bool IsOk { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    /// <remarks>
    /// When this property is <see langword="true"/>, <see cref="Error"/> is
    /// guaranteed to be non-<see langword="null"/>.
    /// </remarks>
    bool IsErr { get; }
}
