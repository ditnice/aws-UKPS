using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records.Dtos.PublishedRecord;

/// <summary>
/// Represents the EU status section of a medicine record.
/// </summary>
public sealed record MedicinesEuStatusDto
{
    /// <summary>
    /// Gets the EU orphan designation status.
    /// </summary>
    public DesignationStatus? EuOrphanStatus { get; init; }

    /// <summary>
    /// Gets the EU orphan designation number.
    /// </summary>
    public string? EuOrphanStatusNumber { get; init; }

    /// <summary>
    /// Gets the date EU orphan status was granted.
    /// </summary>
    public RegulatoryDateDto? EuOrphanGrantedDate { get; init; }

    /// <summary>
    /// Gets the EU advanced therapy medicinal product classification status.
    /// </summary>
    public DesignationStatus? EuAtmpClassificationStatus { get; init; }

    /// <summary>
    /// Gets the ATMP classification recommendation date.
    /// </summary>
    public RegulatoryDateDto? AtmpRecommendationDate { get; init; }

    /// <summary>
    /// Gets the ATMP classification.
    /// </summary>
    public ReferenceDataDto? AtmpClassification { get; init; }
}
