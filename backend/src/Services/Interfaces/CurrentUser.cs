using UKPS.Api.Enums;

namespace UKPS.Api.Services.Interfaces;

/// <summary>
/// Represents the information of the current user of the system.
/// </summary>
public sealed record CurrentUser
{
    /// <summary>
    /// Gets the identifier of the organisation associated with the current user.
    /// </summary>
    public required int OrganisationId { get; init; }

    /// <summary>
    /// Gets the role of the current user within the system.
    /// </summary>
    public required UserRole UserRole { get; init; }
}
