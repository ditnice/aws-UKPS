using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records.Dtos.PublishedRecord;

/// <summary>
/// Represents the development background section of a medicine record.
/// </summary>
public sealed record MedicinesDevelopmentBackgroundDto
{
    /// <summary>
    /// Gets whether this is a repurposed medicine.
    /// </summary>
    public YesNoUnknown? IsRepurposedMedicine { get; init; }

    /// <summary>
    /// Gets the differences from the current licensed indications.
    /// </summary>
    public string? RepurposedMedicineDetails { get; init; }

    /// <summary>
    /// Gets whether the submitting company is the originator.
    /// </summary>
    public YesNoUnknown? IsOriginatorCompany { get; init; }

    /// <summary>
    /// Gets the originator company name.
    /// </summary>
    public string? OriginatorCompanyName { get; init; }

    /// <summary>
    /// Gets whether the product is co-marketed.
    /// </summary>
    public YesNoUnknown? IsCoMarketed { get; init; }

    /// <summary>
    /// Gets the co-marketing company name.
    /// </summary>
    public string? CoMarketingCompanyName { get; init; }
}
