namespace UKPS.Api.Application.Users;

/// <summary>
/// Represents errors that can occur when onboarding a user.
/// </summary>
public abstract record OnboardUserError
{
    /// <summary>
    /// Indicates that the current user is not permitted to onboard a new user.
    /// </summary>
    internal sealed record NotAllowed : OnboardUserError;
}
