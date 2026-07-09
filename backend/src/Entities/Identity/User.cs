using UKPS.Api.Enums;

namespace UKPS.Api.Entities.Identity;

internal sealed record User
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public UserType UserType { get; set; }
    public string? Title { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? JobTitle { get; set; }
    public string? WorkTelephone { get; set; }
    public required string WorkEmail { get; set; }

    // Navigation
    public ICollection<UserOrgMembership> UserOrgMemberships { get; set; } = [];
    public ICollection<UserAudit> UserAudits { get; set; } = [];
}
