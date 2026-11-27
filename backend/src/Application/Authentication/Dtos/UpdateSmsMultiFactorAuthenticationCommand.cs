namespace UKPS.Api.Application.Authentication.Dtos;

/// <summary>
/// Represents a command to update the phone number used for SMS multi-factor authentication.
/// </summary>
public record UpdateSmsMultiFactorAuthenticationCommand
{
    /// <summary>
    /// Gets the new phone number to use for SMS multi-factor authentication.
    /// </summary>
    public required string PhoneNumber { get; init; }
}
