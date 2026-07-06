using UKPS.Api.Enums;

namespace UKPS.Api.DTOs;

public sealed record OrganisationDetailsDto
{
    public int Id { get; init; }
    public required string OrganisationName { get; init; }
    public OrganisationType OrganisationType { get; init; }
    public PharmaceuticalEntity AllowedPharmaceuticalEntity { get; init; }
    public string? CountryOrRegion { get; init; }
    public required string HeadOfficeAddress { get; init; }
    public required string HeadOfficeEmail { get; init; }
    public required string HeadOfficeTelephone { get; init; }
    public UserOrgStatus Status { get; init; }
    public DateTime? LastActive { get; init; }
    public DateTime? CreatedAt { get; init; }
}
