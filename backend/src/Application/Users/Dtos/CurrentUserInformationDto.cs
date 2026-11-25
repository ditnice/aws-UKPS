using System.Diagnostics.CodeAnalysis;

namespace UKPS.Api.Application.Users.Dtos;

/// <summary>
/// Represents the information for the currently authenticated user, including
/// their multi-factor authentication (MFA) settings.
/// </summary>
public sealed record CurrentUserInformationDto : UserInformationDto
{
    /// <summary>
    /// Gets the MFA method in use for the current user.
    /// </summary>
    public required string MfaMethod { get; init; }

    /// <summary>
    /// Gets the obfuscated phone number used for SMS MFA, if applicable.
    /// </summary>
    public required string MfaPhoneNumber { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentUserInformationDto"/> record.
    /// </summary>
    /// <remarks>
    /// Intended for object initializers and serializers. All required members
    /// must be set when using this constructor.
    /// </remarks>
    public CurrentUserInformationDto() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentUserInformationDto"/> record
    /// by copying an existing <see cref="UserInformationDto"/> and adding the
    /// current user's MFA details.
    /// </summary>
    /// <param name="mfaMethod">The MFA method in use for the current user.</param>
    /// <param name="mfaPhoneNumber">
    /// The obfuscated phone number used for SMS MFA, if applicable.
    /// </param>
    /// <param name="userInformationDto">
    /// The base user information to copy into this instance.
    /// </param>
    [SetsRequiredMembers]
    public CurrentUserInformationDto(
        string mfaMethod,
        string mfaPhoneNumber,
        UserInformationDto userInformationDto
    )
        : base(userInformationDto)
    {
        MfaMethod = mfaMethod;
        MfaPhoneNumber = mfaPhoneNumber;
    }
}
