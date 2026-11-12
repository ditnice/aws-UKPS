namespace UKPS.Api.Application.Authentication.Dtos;

/// <summary>
/// Represents the command used to complete user setup by validating a setup token
/// and assigning a phone number to the user for SMS MFA.
/// </summary>
public record SetupSmsMfaCommand
{
    /// <summary>
    /// Gets the unique token used to identify and validate the pending user setup request.
    /// </summary>
    public required Guid SetupToken { get; init; }

    /// <summary>
    /// Gets the phone number to assign to the user account.
    /// </summary>
    public required string PhoneNumber { get; init; }
}
