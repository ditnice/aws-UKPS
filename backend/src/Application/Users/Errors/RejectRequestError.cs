namespace UKPS.Api.Application.Users.Errors;

/// <summary>
/// Represents an error that can occur when rejecting a membership request.
/// </summary>
public abstract record RejectRequestError
{
    /// <summary>
    /// Represents an error indicating that the membership request has already
    /// been approved and therefore cannot be rejected.
    /// </summary>
    public sealed record RegistrationApproved : RejectRequestError;

    /// <summary>
    /// Represents an error indicating that the membership request cannot be
    /// updated because the operation is not allowed.
    /// </summary>
    public record NotAllowed : RejectRequestError;

    /// <summary>
    /// Represents an error indicating that the membership request could not
    /// be found.
    /// </summary>
    public record RequestNotFound : RejectRequestError;

    internal TResult Match<TResult>(
        Func<NotAllowed, TResult> notAllowed,
        Func<RequestNotFound, TResult> requestNotFound,
        Func<RegistrationApproved, TResult> registrationApproved
    )
    {
        return this switch
        {
            NotAllowed e => notAllowed(e),
            RequestNotFound e => requestNotFound(e),
            RegistrationApproved e => registrationApproved(e),
            _ => throw new InvalidOperationException(
                $"Unknown {nameof(RejectRequestError)} type: {GetType().Name}"
            ),
        };
    }
}
