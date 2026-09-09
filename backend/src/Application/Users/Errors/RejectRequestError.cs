namespace UKPS.Api.Application.Users.Errors;

/// <summary>
/// Represents an error that can occur when rejecting a membership request.
/// </summary>
public abstract record RejectRequestError : IMembershipRequestUpdateError
{
    /// <summary>
    /// Represents an error indicating that the membership request cannot be
    /// updated because the operation is not allowed.
    /// </summary>
    public record NotAllowed : RejectRequestError, IMembershipRequestUpdateError.INotAllowed;

    /// <summary>
    /// Represents an error indicating that the membership request could not
    /// be found.
    /// </summary>
    public record RequestNotFound
        : RejectRequestError,
            IMembershipRequestUpdateError.IRequestNotFound;
}
