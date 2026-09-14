using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.Identity;

internal sealed class User
{
    public IReadOnlyCollection<IUserDomainEvent> Events => _events;
    public int Id { get; set; }
    public required CognitoUsername CognitoUsername { get; init; }
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

    private User() { }

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
            membership.FinaliseSetup();
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
            _events.Add(
                new EmailUpdatedEvent() { PreviousWorkEmail = WorkEmail, NewWorkEmail = workEmail }
            );
            WorkEmail = workEmail;
        }
        UpdatedAt = dateTime;
    }

    public static User CreateInitialisedUser(CreateInitialisedUserCommand command)
    {
        var userOnboardingRecord = new UserOnboardingRecord()
        {
            SetupToken = Guid.CreateVersion7(),
            CreatedBy = command.CurrentUserEmail,
            CreatedAt = command.Now,
        };
        var membership = new UserOrgMembership()
        {
            Status = UserOrgMembershipStatus.AwaitingSetup,
            AllowedPharmaceuticalEntity = PharmaceuticalEntity.Both, // URP 435 - Decide what initial value should be set.
            UserRole = UserRole.Standard,
            CreatedAt = command.Now,
            OrganisationId = command.OrganisationId,
        };
        return new User()
        {
            CognitoUsername = command.CognitoUsername,
            FullName = command.FullName,
            WorkEmail = command.WorkEmail,
            WorkTelephone = command.WorkTelephone,
            OnboardingRecord = userOnboardingRecord,
            UserType = command.UserType ?? UserType.PharmaUser,
            CreatedAt = command.Now,
            UserOrgMemberships = [membership],
        };
    }

    internal record EmailUpdatedEvent : IUserDomainEvent
    {
        public required string PreviousWorkEmail { get; init; }
        public required string NewWorkEmail { get; init; }
    }
}
