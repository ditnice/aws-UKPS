namespace UKPS.Api.Application.Authentication.Dtos;

/// <summary>
/// Represents the details required for a user to complete multi-factor authentication setup.
/// </summary>
public record MultiFactorAuthenticationSetupDto
{
    /// <summary>
    /// Gets the OTP authentication URI used to configure an authenticator application.
    /// </summary>
    public required Uri OtpAuthUri { get; init; }

    /// <summary>
    /// Gets the authentication session identifier required to complete the
    /// multi-factor authentication setup flow.
    /// </summary>
    public required string AuthenticationSession { get; init; }
}
