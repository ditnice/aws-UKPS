using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records.Dtos.PublishedRecord;

/// <summary>
/// Represents the patient identification section of a medicine record.
/// </summary>
public sealed record MedicinesPatientIdentificationDto
{
    /// <summary>
    /// Gets whether screening is required.
    /// </summary>
    public YesNoUnknown? ScreeningRequired { get; init; }

    /// <summary>
    /// Gets details of the screening required.
    /// </summary>
    public string? ScreeningDetails { get; init; }

    /// <summary>
    /// Gets whether urgent patient identification is required.
    /// </summary>
    public YesNoUnknown? UrgentIdentificationRequired { get; init; }

    /// <summary>
    /// Gets details of the urgent identification required.
    /// </summary>
    public string? UrgentIdentificationDetails { get; init; }
}
