namespace UKPS.Api.Application.Authentication.Errors;

/// <summary>
/// Represents the authentication challenges that may be returned during the
/// user authentication process.
/// </summary>
public enum UkpsChallengeType
{
    /// <summary>
    /// Indicates that the user must set a new password before authentication
    /// can be completed.
    /// </summary>
    NewPasswordRequired = 0,
}
