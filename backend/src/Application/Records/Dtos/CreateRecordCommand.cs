using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.Application.Records.Dtos;

/// <summary>
/// Contains the details required to create a new record.
/// </summary>
public record CreateRecordCommand
{
    /// <summary>
    /// Gets the identifier of the organisation for which the record will be created.
    /// </summary>
    [Required]
    public int OrganisationId { get; init; }

    /// <summary>
    /// Gets the development names associated with the record.
    /// </summary>
    /// <remarks>
    /// At least one development name must be provided.
    /// </remarks>
    [Required]
    [MinLength(1)]
    public required IReadOnlyCollection<string> DevelopmentNames { get; init; }

    /// <summary>
    /// Gets the optional branded name associated with the record.
    /// </summary>
    public string? BrandedName { get; init; }

    /// <summary>
    /// Gets the generic names associated with the record.
    /// </summary>
    /// <remarks>
    /// At least one generic name must be provided.
    /// </remarks>
    [Required]
    [MinLength(1)]
    public required IReadOnlyCollection<string> GenericNames { get; init; }

    /// <summary>
    /// Gets the title of the record.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public required string RecordTitle { get; init; }
}
