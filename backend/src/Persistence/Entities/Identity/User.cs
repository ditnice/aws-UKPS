using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.Identity;

internal sealed class User
{
    public IReadOnlyCollection<IUserDomainEvent> Events => _events;
    public int Id { get; set; }
    public required UserIdentityId CognitoUsername { get; init; }
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
    public ICollection<UserOrgMembership>? UserOrgMemberships { get; set; }
    public ICollection<UserAudit> UserAudits { get; set; } = [];
    private readonly List<IUserDomainEvent> _events = new List<IUserDomainEvent>();

    internal void FinaliseSetup()
    {
        if (UserOrgMemberships is null)
        {
            throw new InvalidOperationException(
                "Cannot finalise user setup because the user's organisation memberships have not been loaded."
            );
        }

        foreach (var membership in UserOrgMemberships)
        {
            membership.MarkAsActive();
        }
    }

    internal void UpdateDetails(
        string fullName,
        string? workTelephone,
        string workEmail,
        DateTime dateTime
    )
    {
        FullName = fullName;
        WorkTelephone = workTelephone;
        if (!string.Equals(WorkEmail, workEmail, StringComparison.Ordinal))
        {
            _events.Add(new EmailUpdatedEvent());
            WorkEmail = workEmail;
        }
        UpdatedAt = dateTime;
    }

    internal record EmailUpdatedEvent : IUserDomainEvent;
}
