using UKPS.Api.Enums;

namespace UKPS.Api.Entities.Identity;

internal sealed class Organisation
{
    public int Id { get; set; }
    public string OrganisationName { get; set; } = null!;
    public OrganisationType OrganisationType { get; set; }
    public PharmaceuticalEntity AllowedPharmaceuticalEntity { get; set; }
    public string? CountryOrRegion { get; set; }
    public string HeadOfficeAddressLine1 { get; set; } = null!;
    public string? HeadOfficeAddressLine2 { get; set; }
    public string HeadOfficeTown { get; set; } = null!;
    public string? HeadOfficeCounty { get; set; }
    public string HeadOfficePostcode { get; set; } = null!;
    public string HeadOfficeTelephone { get; set; } = null!;
    public string HeadOfficeEmail { get; set; } = null!;
    public UserOrgStatus Status { get; set; }
    public DateTime? LastActive { get; set; }
    public DateTime? CreatedAt { get; set; }

    // Navigation
    public ICollection<TermsAcceptance> TermsAcceptances { get; set; } = [];
    public ICollection<OrganisationAudit> OrganisationAudits { get; set; } = [];
    public ICollection<UserOrgMembership> UserOrgMemberships { get; set; } = [];
}
