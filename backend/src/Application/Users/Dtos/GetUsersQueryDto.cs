using System.ComponentModel.DataAnnotations;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Users.Dtos;

/// <summary>
/// Represents the data transfer object for querying users.
/// </summary>
public sealed record GetUsersQueryDto
{
    /// <summary>
    /// Gets or initialises the optional ID of the organisation to filter users by.
    /// </summary>
    public int? OrganisationId { get; init; }

    /// <summary>
    /// Gets or initialises the page number for pagination.
    /// Must be greater than or equal to 1.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page cannot be less than 1.")]
    public int Page { get; init; } = 1;

    /// <summary>
    /// Gets or initialises the number of items per page for pagination.
    /// Must be between 1 and 100.
    /// </summary>
    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Gets or initialises the collection of user organisation statuses to filter users by.
    /// Users with a <see cref="UserOrgStatus.Rejected"/> status are always excluded from
    /// results, regardless of the values supplied here.
    /// </summary>
    public ICollection<UserOrgStatus> Status { get; init; } = [];

    /// <summary>
    /// Gets or initialises the collection of user roles to filter users by.
    /// </summary>
    public ICollection<UserRole> Role { get; init; } = [];

    /// <summary>
    /// Gets or initialises an optional partial, case-insensitive match against the user's email address.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Gets or initialises the inclusive lower bound of the user's last active timestamp to filter by.
    /// </summary>
    public DateTimeOffset? LastActiveFrom { get; init; }

    /// <summary>
    /// Gets or initialises the inclusive upper bound of the user's last active timestamp to filter by.
    /// </summary>
    public DateTimeOffset? LastActiveTo { get; init; }
}
