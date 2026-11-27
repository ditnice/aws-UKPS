namespace UKPS.Api.Application.Authentication.Dtos;

/// <summary>
/// Represents a command to verify an SMS multi-factor authentication phone number update.
/// </summary>
public record VerifySmsMultiFactorAuthenticationUpdateCommand
{
    /// <summary>
    /// Gets the verification code sent to the new phone number.
    /// </summary>
    public required string Code { get; init; }
}
