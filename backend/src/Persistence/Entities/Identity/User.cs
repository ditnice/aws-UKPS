using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.Identity;

internal sealed class User
{
    public int Id { get; set; }
    public string? IdentityId { get; init; }
    public UserType UserType { get; set; }
    public string? Title { get; set; }
    public required string FullName { get; set; }
    public string? JobTitle { get; set; }
    public string? WorkTelephone { get; set; }
    public required string WorkEmail { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastActive { get; set; }

    public UserOnboardingRecord? OnboardingRecord { get; init; }

    // Navigation
    public ICollection<UserOrgMembership> UserOrgMemberships { get; set; } = [];
    public ICollection<UserAudit> UserAudits { get; set; } = [];
}
