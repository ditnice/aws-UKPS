namespace UKPS.Api.Application.AuthorisationAdministration;

/// <summary>
/// Represents the details required for a user to complete multi-factor authentication setup.
/// </summary>
public record MultiFactorAuthenticationSetupDto
{
    /// <summary>
    /// Gets the OTP authentication URI used to configure an authenticator application.
    /// </summary>
    public required Uri OtpAuthUri { get; init; }
}
