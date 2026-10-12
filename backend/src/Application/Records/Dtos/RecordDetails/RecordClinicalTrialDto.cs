using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records.Dtos.RecordDetails;

/// <summary>
/// Represents a clinical trial on a record.
/// </summary>
public sealed record RecordClinicalTrialDto
{
    /// <summary>
    /// Gets the study name.
    /// </summary>
    public required string StudyName { get; init; }

    /// <summary>
    /// Gets the ClinicalTrials.gov number.
    /// </summary>
    public required string ClinicalTrialsGovNumber { get; init; }

    /// <summary>
    /// Gets other registry numbers for the trial, e.g. ISRCTN or EudraCT, in display order.
    /// </summary>
    public required IReadOnlyCollection<string> OtherClinicalTrialNumbers { get; init; }

    /// <summary>
    /// Gets the trial phase.
    /// </summary>
    public TrialPhase? TrialPhase { get; init; }

    /// <summary>
    /// Gets the brief description of key findings.
    /// </summary>
    public string? BriefDescription { get; init; }
}
