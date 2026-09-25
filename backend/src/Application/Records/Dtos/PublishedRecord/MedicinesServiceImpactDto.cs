using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records.Dtos.PublishedRecord;

/// <summary>
/// Represents the NHS service impact section of a medicine record.
/// </summary>
public sealed record MedicinesServiceImpactDto
{
    /// <summary>
    /// Gets whether NHS service changes are required.
    /// </summary>
    public NhsServiceChangesRequired? NhsServiceChangesRequired { get; init; }

    /// <summary>
    /// Gets details of the NHS service changes.
    /// </summary>
    public string? NhsServiceChangesDetails { get; init; }

    /// <summary>
    /// Gets whether there are handling or storage requirements.
    /// </summary>
    public YesNoUnknown? HandlingStorageRequirements { get; init; }

    /// <summary>
    /// Gets details of the handling or storage requirements.
    /// </summary>
    public string? HandlingStorageDetails { get; init; }

    /// <summary>
    /// Gets the estimated uptake.
    /// </summary>
    public string? EstimatedUptake { get; init; }

    /// <summary>
    /// Gets the UK patient population range.
    /// </summary>
    public ReferenceDataDto? UkPatientPopulationRange { get; init; }

    /// <summary>
    /// Gets notes on the UK patient population.
    /// </summary>
    public string? UkPatientPopulationNotes { get; init; }

    /// <summary>
    /// Gets the estimated eligible patient population.
    /// </summary>
    public string? EstimatedEligiblePatientPopulation { get; init; }

    /// <summary>
    /// Gets whether compassionate access is available.
    /// </summary>
    public YesNoUnknown? CompassionateAccessAvailable { get; init; }

    /// <summary>
    /// Gets details of compassionate access.
    /// </summary>
    public string? CompassionateAccessDetails { get; init; }
}
