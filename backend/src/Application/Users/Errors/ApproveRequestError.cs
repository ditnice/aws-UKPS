namespace UKPS.Api.Application.Users.Errors;

/// <summary>
/// Represents an error that can occur when approving a membership request.
/// </summary>
public abstract record ApproveRequestError
{
    /// <summary>
    /// Represents an error indicating that the registration request has already
    /// been rejected.
    /// </summary>
    public sealed record RegistrationRejected : ApproveRequestError;

    /// <summary>
    /// Represents an error indicating that the requester is already a member
    /// of the organisation.
    /// </summary>
    public sealed record UserAlreadyInOrganisation : ApproveRequestError;

    /// <summary>
    /// Represents an error indicating that the specified organisation is invalid
    /// or does not exist.
    /// </summary>
    public sealed record InvalidOrganisation : ApproveRequestError;

    /// <summary>
    /// Represents an error indicating that the membership request cannot be
    /// updated because the operation is not allowed.
    /// </summary>
    public sealed record NotAllowed : ApproveRequestError;

    /// <summary>
    /// Represents an error indicating that the membership request could not
    /// be found.
    /// </summary>
    public sealed record RequestNotFound : ApproveRequestError;

    /// <summary>
    /// Represents an error indicating that the membership request could not be
    /// approved because the operation conflicted with another update.
    /// </summary>
    public sealed record ConcurrencyError : ApproveRequestError;

    internal TResult Match<TResult>(
        Func<NotAllowed, TResult> notAllowed,
        Func<RequestNotFound, TResult> requestNotFound,
        Func<RegistrationRejected, TResult> registrationRejected,
        Func<UserAlreadyInOrganisation, TResult> userAlreadyInOrganisation,
        Func<InvalidOrganisation, TResult> invalidOrganisation,
        Func<ConcurrencyError, TResult> concurrencyError
    )
    {
        return this switch
        {
            NotAllowed e => notAllowed(e),
            RequestNotFound e => requestNotFound(e),
            RegistrationRejected e => registrationRejected(e),
            UserAlreadyInOrganisation e => userAlreadyInOrganisation(e),
            InvalidOrganisation e => invalidOrganisation(e),
            ConcurrencyError e => concurrencyError(e),
            _ => throw new InvalidOperationException(
                $"Unknown {nameof(ApproveRequestError)} type: {GetType().Name}"
            ),
        };
    }
}
