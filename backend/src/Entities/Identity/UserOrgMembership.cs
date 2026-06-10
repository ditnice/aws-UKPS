using UKPS.Data.Enums;

namespace UKPS.Data.Entities.Identity;

public class UserOrgMembership
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int OrganisationId { get; set; }
    public UserRole UserRole { get; set; }
    public UserOrgStatus Status { get; set; }
    public PharmaceuticalEntity AllowedPharmaceuticalEntity { get; set; }
    public DateTime? CreatedAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Organisation Organisation { get; set; } = null!;
}
