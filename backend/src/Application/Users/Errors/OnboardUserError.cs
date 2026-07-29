namespace UKPS.Api.Application.Users.Errors;

/// <summary>
/// Represents errors that can occur when onboarding a user.
/// </summary>
public abstract record OnboardUserError
{
    /// <summary>
    /// Prevents direct instantiation of onboarding errors.
    /// </summary>
    protected OnboardUserError() { }

    /// <summary>
    /// Indicates that a user with the specified username already exists.
    /// </summary>
    public sealed record UsernameAlreadyExists : OnboardUserError;

    /// <summary>
    /// Indicates that the specified organisation does not exist.
    /// </summary>
    public sealed record InvalidOrganisation : OnboardUserError;

    /// <summary>
    /// Indicates that the current user is not permitted to onboard a new user.
    /// </summary>
    public sealed record NotAllowed : OnboardUserError;

    internal TResult Match<TResult>(
        Func<UsernameAlreadyExists, TResult> usernameAlreadyExists,
        Func<InvalidOrganisation, TResult> invalidOrganisation,
        Func<NotAllowed, TResult> notAllowed
    )
    {
        return this switch
        {
            UsernameAlreadyExists x => usernameAlreadyExists(x),
            InvalidOrganisation x => invalidOrganisation(x),
            NotAllowed x => notAllowed(x),
            _ => throw new InvalidOperationException("Unknown onboarding user error."),
        };
    }
}
