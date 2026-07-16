using UKPS.Api.Enums;

namespace UKPS.Api.Entities.Identity;

internal sealed class UserOrgMembership
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int OrganisationId { get; set; }
    public UserRole UserRole { get; set; }
    public UserOrgStatus Status { get; set; }
    public PharmaceuticalEntity AllowedPharmaceuticalEntity { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public User? User { get; set; }
    public Organisation? Organisation { get; set; }
}
