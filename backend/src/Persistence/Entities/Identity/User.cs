using System.Diagnostics.CodeAnalysis;
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

    internal int? FindCurrentOrganisationId()
    {
        if (UserOrgMemberships is null)
        {
            throw new InvalidOperationException(
                "Cannot get current user because the user's organisation memberships have not been loaded."
            );
        }

        UserOrgMembership? membership =
            UserOrgMemberships.Count == 1
                ? UserOrgMemberships.Single()
                : UserOrgMemberships.FirstOrDefault(x => x.IsSelectedAsCurrentOrganisation);

        if (membership is null)
        {
            return null;
        }

        return membership.OrganisationId;
    }

    internal void FinaliseSetup()
    {
        GuardAgainstUserMembershipsNotLoaded();

        foreach (var membership in UserOrgMemberships)
        {
            membership.FinaliseSetup();
        }
    }

    internal bool TryUpdateCurrentOrganisation(int organisationId)
    {
        GuardAgainstUserMembershipsNotLoaded();

        bool currentOrganisationAlreadySet = UserOrgMemberships.Any(x =>
            x.IsSelectedAsCurrentOrganisation
        );
        if (currentOrganisationAlreadySet)
        {
            throw new InvalidOperationException(
                "Current organisation is already set. Call ResetCurrentOrganisation first in a separate database operation."
            );
        }

        var foundMembership = UserOrgMemberships.FirstOrDefault(x =>
            x.OrganisationId == organisationId && x.IsAuthorised()
        );

        if (foundMembership is null)
        {
            return false;
        }

        foundMembership.IsSelectedAsCurrentOrganisation = true;

        return true;
    }

    internal void ResetCurrentOrganisation()
    {
        GuardAgainstUserMembershipsNotLoaded();

        foreach (var membership in UserOrgMemberships)
        {
            membership.IsSelectedAsCurrentOrganisation = false;
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

    [MemberNotNull(nameof(UserOrgMemberships))]
    private void GuardAgainstUserMembershipsNotLoaded()
    {
        if (UserOrgMemberships is null)
        {
            throw new InvalidOperationException(
                "Cannot perform operation because the user's organisation memberships have not been loaded."
            );
        }
    }

    internal record EmailUpdatedEvent : IUserDomainEvent
    {
        public required string PreviousWorkEmail { get; init; }
        public required string NewWorkEmail { get; init; }
    }
}
