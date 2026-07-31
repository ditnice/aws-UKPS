namespace UKPS.Api.Application.InternalServices.Identity;

/// <summary>
/// Represents the result of associating a software token with a user account
/// during multi-factor authentication setup.
/// </summary>
public record AssociateSoftwareTokenResult
{
    /// <summary>
    /// Gets the secret key used to configure an authenticator application.
    /// </summary>
    public required string Secret { get; init; }

    /// <summary>
    /// Gets the authentication session identifier required to complete the
    /// software token association flow.
    /// </summary>
    public required string AuthenticationSession { get; init; }
}
