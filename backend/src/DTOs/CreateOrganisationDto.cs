namespace UKPS.Api.DTOs;

/// <summary>
/// Represents the information required to create a new organisation.
/// </summary>
public record CreateOrganisationDto
{
    /// <summary>
    /// Gets or sets the name of the organisation.
    /// </summary>
    public required string OrganisationName { get; set; }

    /// <summary>
    /// Gets or sets the country or region where the organisation is based, if applicable.
    /// </summary>
    public string? CountryOrRegion { get; set; }

    /// <summary>
    /// Gets or sets the head office address of the organisation.
    /// </summary>
    public required string HeadOfficeAddress { get; set; }

    /// <summary>
    /// Gets or sets the head office email address of the organisation.
    /// </summary>
    public required string HeadOfficeEmail { get; set; }

    /// <summary>
    /// Gets or sets the head office telephone number of the organisation.
    /// </summary>
    public required string HeadOfficeTelephone { get; set; }
}
