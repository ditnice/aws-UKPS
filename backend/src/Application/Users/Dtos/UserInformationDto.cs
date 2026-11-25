using System.Diagnostics.CodeAnalysis;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Users.Dtos;

/// <summary>
/// Represents information about a user and their membership within an organisation.
/// </summary>
public record UserInformationDto
{
    /// <summary>
    /// Gets the unique identifier of the user.
    /// </summary>
    public required int UserId { get; init; }

    /// <summary>
    /// Gets the full name of the user.
    /// </summary>
    public required string FullName { get; init; }

    /// <summary>
    /// Gets the user's work telephone number.
    /// </summary>
    public required string WorkTelephone { get; init; }

    /// <summary>
    /// Gets the user's email address.
    /// </summary>
    public required string WorkEmail { get; init; }

    /// <summary>
    /// Gets the unique identifier of the user's organisation membership.
    /// </summary>
    public required int OrganisationMembershipId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the user's organisation.
    /// </summary>
    public required int OrganisationId { get; init; }

    /// <summary>
    /// Gets the name of the user's organisation.
    /// </summary>
    public required string OrganisationName { get; init; }

    /// <summary>
    /// Gets the role assigned to the user within the organisation.
    /// </summary>
    public required UserRole UserRole { get; init; }

    /// <summary>
    /// Gets the user's membership status within the organisation.
    /// </summary>
    public required UserOrgStatus Status { get; init; }

    /// <summary>
    /// Gets whether the user can select more than one organisation. Only set for the current user.
    /// </summary>
    public bool HasMultipleOrganisations { get; init; }

    /// <summary>
    /// Gets the permissions of the user. Whether they can interact with medicines, vaccines or both.
    /// </summary>
    public required PharmaceuticalEntity AllowedPharmaceuticalEntity { get; init; }

    /// <summary>
    /// Gets the details of a requested change to the user's work email address that has not yet been
    /// completed, or <see langword="null"/> if there is no pending change.
    /// </summary>
    public PendingWorkEmailChangeDto? PendingWorkEmail { get; init; }

    /// <summary>
    /// Initialises a new instance of the <see cref="UserInformationDto"/> record.
    /// </summary>
    public UserInformationDto() { }

    /// <summary>
    /// Initialises a new instance of the <see cref="UserInformationDto"/> record by copying
    /// the values from an existing instance.
    /// </summary>
    /// <param name="userInformation">The user information to copy.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="userInformation"/> is <see langword="null"/>.
    /// </exception>
    [SetsRequiredMembers]
    public UserInformationDto(UserInformationDto userInformation)
    {
        ArgumentNullException.ThrowIfNull(userInformation);

        UserId = userInformation.UserId;
        FullName = userInformation.FullName;
        WorkTelephone = userInformation.WorkTelephone;
        WorkEmail = userInformation.WorkEmail;
        OrganisationMembershipId = userInformation.OrganisationMembershipId;
        OrganisationId = userInformation.OrganisationId;
        OrganisationName = userInformation.OrganisationName;
        UserRole = userInformation.UserRole;
        Status = userInformation.Status;
        HasMultipleOrganisations = userInformation.HasMultipleOrganisations;
        AllowedPharmaceuticalEntity = userInformation.AllowedPharmaceuticalEntity;
        PendingWorkEmail = userInformation.PendingWorkEmail;
    }
}
