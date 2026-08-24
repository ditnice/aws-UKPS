using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.Identity;

internal sealed class UserOrgMembership
{
    public int Id { get; set; }
    public required UserRole UserRole { get; set; }
    public required UserOrgStatus Status
    {
        get => _status;
        init => _status = value;
    }
    public required PharmaceuticalEntity AllowedPharmaceuticalEntity { get; set; }
    public required DateTime CreatedAt { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }
    public int OrganisationId { get; set; }
    public Organisation? Organisation { get; set; }

    private UserOrgStatus _status;

    internal bool IsAuthorised()
    {
        UserOrgStatus[] authorisedStatuses = [UserOrgStatus.Active, UserOrgStatus.Inactive];
        return authorisedStatuses.Contains(Status);
    }

    internal void MarkAsActive()
    {
        UserOrgStatus[] invalidInitialStates = [UserOrgStatus.Deactivated, UserOrgStatus.Rejected];

        if (invalidInitialStates.Contains(Status))
        {
            throw new InvalidOperationException(
                $"User organisation membership cannot be marked as active when its current status is '{Status}'."
            );
        }
        _status = UserOrgStatus.Active;
    }

    internal void Deactivate()
    {
        UserOrgStatus[] validInitialStates = [UserOrgStatus.Deactivated, UserOrgStatus.Rejected];

        if (!validInitialStates.Contains(Status))
        {
            throw new InvalidOperationException(
                $"User organisation membership cannot be marked be deactivated when its current status is '{Status}'."
            );
        }

        _status = UserOrgStatus.Deactivated;
    }
}
