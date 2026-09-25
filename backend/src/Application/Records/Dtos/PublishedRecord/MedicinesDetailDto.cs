using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records.Dtos.PublishedRecord;

/// <summary>
/// Represents the medicine detail section of a medicine record.
/// </summary>
public sealed record MedicinesDetailDto
{
    /// <summary>
    /// Gets the mode of action.
    /// </summary>
    public string? ModeOfAction { get; init; }

    /// <summary>
    /// Gets the proposed dose regimen.
    /// </summary>
    public string? ProposedDoseRegimen { get; init; }

    /// <summary>
    /// Gets whether this is a personalised medicine.
    /// </summary>
    public YesNoUnknown? IsPersonalisedMedicine { get; init; }

    /// <summary>
    /// Gets whether this is a repurposed medicine.
    /// </summary>
    public YesNoUnknown? IsRepurposedMedicine { get; init; }

    /// <summary>
    /// Gets details of the repurposed medicine.
    /// </summary>
    public string? RepurposedMedicineDetails { get; init; }
}
