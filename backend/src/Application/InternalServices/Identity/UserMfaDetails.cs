// backend/src/Application/InternalServices/Identity/UserMfaDetails.cs
namespace UKPS.Api.Application.InternalServices.Identity;

/// <summary>
/// Describes the MFA configuration currently in effect for a Cognito user.
/// </summary>
internal sealed record UserMfaDetails
{
    /// <summary>
    /// The MFA method in use: "Sms" or "AuthenticatorApp".
    /// </summary>
    public required string MfaMethod { get; init; }

    /// <summary>
    /// The user's phone number, obfuscated for display (e.g. "07** ***89").
    /// Empty when no phone number is stored or SMS is not the method.
    /// </summary>
    public required string MfaPhoneNumber { get; init; }
}
