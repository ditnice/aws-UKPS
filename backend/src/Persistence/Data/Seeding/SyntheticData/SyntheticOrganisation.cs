using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Data.Seeding.SyntheticData;

internal sealed record SyntheticOrganisation
{
    public required string OrganisationName { get; init; }
    public required OrganisationType OrganisationType { get; init; }
    public required PharmaceuticalEntity AllowedPharmaceuticalEntity { get; init; }
    public string? CountryOrRegion { get; init; }
    public required string HeadOfficeAddress { get; init; }
    public required string HeadOfficeTelephone { get; init; }
    public required string HeadOfficeEmail { get; init; }
    public required UserOrgStatus Status { get; init; }
    public required DateOnly CreatedAt { get; init; }
    public DateOnly? LastActive { get; init; }
}
