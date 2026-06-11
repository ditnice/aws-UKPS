using UKPS.Api.Enums;

namespace UKPS.Api.Entities.Identity;

internal sealed class Organisation
{
    public int Id { get; set; }
    public string OrganisationName { get; set; } = null!;
    public OrganisationType OrganisationType { get; set; }
    public PharmaceuticalEntity AllowedPharmaceuticalEntity { get; set; }
    public string? CountryOrRegion { get; set; }
    public string? HeadOfficeAddress { get; set; }
    public string? HeadOfficeTelephone { get; set; }
    public string? HeadOfficeEmail { get; set; }
    public UserOrgStatus Status { get; set; }
    public DateTime? LastActive { get; set; }
    public DateTime? CreatedAt { get; set; }

    // Navigation
    public ICollection<TermsAcceptance> TermsAcceptances { get; set; } = [];
    public ICollection<OrganisationAudit> OrganisationAudits { get; set; } = [];
    public ICollection<UserOrgMembership> UserOrgMemberships { get; set; } = [];
}
