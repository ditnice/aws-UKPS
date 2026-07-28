namespace UKPS.Api.Application.Authentication.Errors;

/// <summary>
/// Represents the authentication challenges that may be returned during the
/// user authentication process.
/// </summary>
public enum UkpsChallengeType
{
    /// <summary>
    /// Indicates that the user must complete a multi-factor authentication
    /// challenge before authentication can continue.
    /// </summary>
    MultiFactorAuthenticationRequired = 0,
}
