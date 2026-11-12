namespace UKPS.Api.Application.Authentication.Dtos;

/// <summary>
/// Represents the details required for a user to complete SMS multi-factor authentication setup.
/// </summary>
public record SmsMultiFactorAuthenticationSetupDto
{
    /// <summary>
    /// Gets the authentication session identifier required to complete the
    /// multi-factor authentication setup flow.
    /// </summary>
    public required string AuthenticationSession { get; init; }
}
