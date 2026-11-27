using System.Diagnostics.CodeAnalysis;

namespace UKPS.Api.Application.Users.Dtos;

/// <summary>
/// Represents the information for the currently authenticated user, including
/// their multi-factor authentication (MFA) settings.
/// </summary>
public sealed record CurrentUserInformationDto : CompleteUserInformationDto
{
    /// <inheritdoc cref="CompleteUserInformationDto()"/>
    public CurrentUserInformationDto() { }

    /// <inheritdoc cref="CompleteUserInformationDto(string, string, bool, UserInformationDto)"/>
    [SetsRequiredMembers]
    public CurrentUserInformationDto(
        string mfaMethod,
        string mfaPhoneNumber,
        bool hasMultipleOrganisations,
        UserInformationDto userInformationDto
    )
        : base(mfaMethod, mfaPhoneNumber, hasMultipleOrganisations, userInformationDto) { }
}
