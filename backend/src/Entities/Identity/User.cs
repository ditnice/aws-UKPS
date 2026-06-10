using UKPS.Data.Enums;

namespace UKPS.Data.Entities.Identity;

public class User
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

    // TODO: Cognito-managed placeholders — to be replaced when implementing Cognito
    public DateTime? LoginTime { get; set; }
    public DateTime? LogoutTime { get; set; }
    public DateTime? LastActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<UserOrgMembership> UserOrgMemberships { get; set; } = [];
    public ICollection<UserAudit> UserAudits { get; set; } = [];
}
