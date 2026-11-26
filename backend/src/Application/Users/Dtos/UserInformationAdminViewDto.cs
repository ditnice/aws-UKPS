using System.Diagnostics.CodeAnalysis;

namespace UKPS.Api.Application.Users.Dtos;

/// <summary>
/// Represents the information about a user within an organisation as viewed by a super user,
/// including their multi-factor authentication (MFA) settings.
/// </summary>
public sealed record UserInformationAdminViewDto : CompleteUserInformationDto
{
    /// <inheritdoc cref="CompleteUserInformationDto()"/>
    public UserInformationAdminViewDto() { }

    /// <inheritdoc cref="CompleteUserInformationDto(string, string, bool, UserInformationDto)"/>
    [SetsRequiredMembers]
    public UserInformationAdminViewDto(
        string mfaMethod,
        string mfaPhoneNumber,
        bool hasMultipleOrganisations,
        UserInformationDto userInformationDto
    )
        : base(mfaMethod, mfaPhoneNumber, hasMultipleOrganisations, userInformationDto) { }
}
