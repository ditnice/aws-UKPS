using UKPS.Api.Enums;

namespace UKPS.Api.Entities.Identity;

internal sealed class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public UserType UserType { get; set; }
    public string? Title { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? JobTitle { get; set; }
    public string? WorkTelephone { get; set; }
    public string? WorkEmail { get; set; }

    // Navigation
    public ICollection<UserOrgMembership> UserOrgMemberships { get; set; } = [];
    public ICollection<UserAudit> UserAudits { get; set; } = [];
}
