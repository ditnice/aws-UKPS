using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.Application.Users.Dtos;

/// <summary>
/// Represents the request to update the currently authenticated user's active organisation.
/// </summary>
public sealed record UpdateCurrentOrganisationCommand
{
    /// <summary>
    /// Gets the identifier of the organisation to set as the current organisation.
    /// </summary>
    [Required]
    public required int OrganisationId { get; init; }
}
