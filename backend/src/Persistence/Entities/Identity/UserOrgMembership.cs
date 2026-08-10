using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.Identity;

internal sealed class UserOrgMembership
{
    public int Id { get; set; }
    public required UserRole UserRole { get; set; }
    public required UserOrgStatus Status { get; set; }
    public required PharmaceuticalEntity AllowedPharmaceuticalEntity { get; set; }
    public required DateTime CreatedAt { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }
    public int OrganisationId { get; set; }
    public Organisation? Organisation { get; set; }

    internal bool IsAuthorised()
    {
        UserOrgStatus[] authorisedStatuses = [UserOrgStatus.Active, UserOrgStatus.Inactive];
        return authorisedStatuses.Contains(Status);
    }
}
