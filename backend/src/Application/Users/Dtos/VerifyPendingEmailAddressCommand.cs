using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.Application.Users.Dtos;

/// <summary>
/// Command to verify a user's pending email address using a previously issued verification token.
/// </summary>
public sealed record VerifyPendingEmailAddressCommand
{
    /// <summary>
    /// The verification token that was sent to the pending email address.
    /// </summary>
    [Required]
    public required string VerificationToken { get; init; }
}
