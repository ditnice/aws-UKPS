namespace UKPS.Api.DTOs;

public record CreateOrganisationDto
{
    public required string OrganisationName { get; set; }
    public string? CountryOrRegion { get; set; }
    public required string HeadOfficeAddress { get; set; }
    public required string HeadOfficeEmail { get; set; }
    public required string HeadOfficeTelephone { get; set; }
}
