using System.ComponentModel.DataAnnotations;
using UKPS.Api.WebApi.Validators;

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
    public required int OrganisationId { get; init; }

    /// <summary>
    /// Gets the company's internal code or working name for the product, e.g. ABC-123.
    /// </summary>
    [Required]
    public required string CompanyCode { get; init; }

    /// <summary>
    /// Gets the optional branded name associated with the record.
    /// </summary>
    /// <remarks>
    /// This field is optional, but when provided it cannot be empty or whitespace.
    /// </remarks>
    [NotEmptyOrWhitespace]
    public string? BrandedName { get; init; }

    /// <summary>
    /// Gets the generic names associated with the record.
    /// </summary>
    /// <remarks>
    /// At least one generic name must be provided, and names cannot be empty or whitespace.
    /// Names must be distinct, ignoring case and leading or trailing whitespace.
    /// </remarks>
    [Required]
    [MinLength(1)]
    [DistinctStrings(StringComparison.OrdinalIgnoreCase)]
    [NoEmptyOrWhitespaceItems]
    public required IReadOnlyCollection<string> GenericNames { get; init; }

    /// <summary>
    /// Gets the other names, codes, or synonyms associated with the record.
    /// </summary>
    [NoEmptyOrWhitespaceItems]
    public IReadOnlyCollection<string> OtherIdentifiers { get; init; } = [];

    /// <summary>
    /// Gets the title of the record.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [MaxLength(100)]
    public required string RecordTitle { get; init; }
}
