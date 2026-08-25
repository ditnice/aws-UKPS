using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Users.Dtos;

/// <summary>
/// Represents information about the currently authenticated user and their
/// membership within an organisation.
/// </summary>
public record CurrentUserInformationDto
{
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
    public required string EmailAddress { get; init; }

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
}
