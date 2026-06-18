using UKPS.Api.Enums;

namespace UKPS.Api.DTOs;

public sealed class OrganisationDetailsDto
{
    public int Id { get; init; }
    public string OrganisationName { get; init; } = null!;
    public OrganisationType OrganisationType { get; init; }
    public PharmaceuticalEntity AllowedPharmaceuticalEntity { get; init; }
    public string? CountryOrRegion { get; init; }
    public string? HeadOfficeAddress { get; init; }
    public string? HeadOfficeEmail { get; init; }
    public string? HeadOfficeTelephone { get; init; }
    public UserOrgStatus Status { get; init; }
    public DateTime? LastActive { get; init; }
    public DateTime? CreatedAt { get; init; }
}
