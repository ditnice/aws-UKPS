using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.DTOs;

public sealed class UpdateOrganisationDetailsDto
{
    [Required]
    public string OrganisationName { get; init; } = null!;

    [Required]
    public string HeadOfficeAddressLine1 { get; init; } = null!;

    public string? HeadOfficeAddressLine2 { get; init; }

    [Required]
    public string HeadOfficeTown { get; init; } = null!;

    public string? HeadOfficeCounty { get; init; }

    [Required]
    public string HeadOfficePostcode { get; init; } = null!;

    [Required]
    [EmailAddress]
    public string HeadOfficeEmail { get; init; } = null!;

    [Required]
    public string HeadOfficeTelephone { get; init; } = null!;
}
