using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records.Dtos.PublishedRecord;

/// <summary>
/// Represents the company information section of a medicine record.
/// </summary>
public sealed record MedicinesCompanyInfoDto
{
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
