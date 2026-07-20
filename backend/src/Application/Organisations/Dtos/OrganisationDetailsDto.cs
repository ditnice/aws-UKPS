using UKPS.Api.Enums;

namespace UKPS.Api.Application.Organisations.Dtos;

/// <summary>
/// Represents the details of an organisation.
/// </summary>
public sealed record OrganisationDetailsDto
{
    /// <summary>
    /// Gets the unique identifier of the organisation.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Gets the name of the organisation.
    /// </summary>
    public required string OrganisationName { get; init; }

    /// <summary>
    /// Gets the type of the organisation.
    /// </summary>
    public OrganisationType OrganisationType { get; init; }

    /// <summary>
    /// Gets the allowed pharmaceutical entity for the organisation.
    /// </summary>
    public PharmaceuticalEntity AllowedPharmaceuticalEntity { get; init; }

    /// <summary>
    /// Gets the country or region where the organisation is located.
    /// </summary>
    public string? CountryOrRegion { get; init; }

    /// <summary>
    /// Gets the address of the organisation's head office.
    /// </summary>
    public required string HeadOfficeAddress { get; init; }

    /// <summary>
    /// Gets the email address of the organisation's head office.
    /// </summary>
    public required string HeadOfficeEmail { get; init; }

    /// <summary>
    /// Gets the telephone number of the organisation's head office.
    /// </summary>
    public required string HeadOfficeTelephone { get; init; }

    /// <summary>
    /// Gets the current status of the organisation.
    /// </summary>
    public UserOrgStatus Status { get; init; }

    /// <summary>
    /// Gets the date and time when the organisation was last active.
    /// </summary>
    public DateTime? LastActive { get; init; }

    /// <summary>
    /// Gets the date and time when the organisation was created.
    /// </summary>
    public DateTime? CreatedAt { get; init; }
}
