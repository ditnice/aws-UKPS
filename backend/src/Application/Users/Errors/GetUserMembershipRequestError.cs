namespace UKPS.Api.Application.Users.Errors;

/// <summary>
/// Represents an error that can occur when retrieving a user's membership request.
/// </summary>
public abstract record GetUserMembershipRequestError
{
    /// <summary>
    /// Indicates that the requested membership request could not be found.
    /// </summary>
    public sealed record NotFound : GetUserMembershipRequestError;

    /// <summary>
    /// Indicates that the current user is not permitted to retrieve the membership request.
    /// </summary>
    public sealed record NotAllowed : GetUserMembershipRequestError;

    internal TResult Match<TResult>(
        Func<NotFound, TResult> notFound,
        Func<NotAllowed, TResult> notAllowed
    )
    {
        return this switch
        {
            NotFound error => notFound(error),
            NotAllowed error => notAllowed(error),
            _ => throw new InvalidOperationException($"Unknown error type: {GetType().Name}"),
        };
    }
}
