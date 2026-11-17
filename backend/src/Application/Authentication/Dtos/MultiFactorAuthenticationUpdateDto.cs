namespace UKPS.Api.Application.Authentication.Dtos;

/// <summary>
/// Represents the details required for a user to complete multi-factor authentication update.
/// </summary>
public record MultiFactorAuthenticationUpdateDto
{
    /// <summary>
    /// Gets the OTP authentication URI used to configure an authenticator application.
    /// </summary>
    public required Uri OtpAuthUri { get; init; }
}
