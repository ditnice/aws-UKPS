using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Organisations.Dtos;

/// <summary>
/// Represents the data transfer object for an organisation membership.
/// </summary>
public record OrganisationMembershipDto
{
    /// <summary>
    /// Gets the unique identifier of the organisation membership.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the unique identifier of the user associated with the membership.
    /// </summary>
    public required int UserId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the organisation associated with the membership.
    /// </summary>
    public required int OrganisationId { get; init; }

    /// <summary>
    /// Gets the role of the user within the organisation.
    /// </summary>
    public required UserRole UserRole { get; init; }

    /// <summary>
    /// Gets the status of the user within the organisation.
    /// </summary>
    public required UserOrgStatus Status { get; init; }

    /// <summary>
    /// Gets the pharmaceutical entity that the user is allowed to access.
    /// </summary>
    public required PharmaceuticalEntity AllowedPharmaceuticalEntity { get; init; }

    /// <summary>
    /// Gets the date and time when the organisation membership was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }
}
