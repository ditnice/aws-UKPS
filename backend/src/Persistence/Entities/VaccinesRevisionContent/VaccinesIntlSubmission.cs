using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.VaccinesRevisionContent;

/// <summary>
/// Vaccines-only international regulatory section. Replaces the structured
/// MedicinesIntlRecognition approach with a simple two-field section.
/// </summary>
internal sealed class VaccinesIntlSubmission
{
    public int Id { get; set; }
    public int RevisionId { get; set; }

    /// <summary>
    /// Has this vaccine been, or is it intended to be, submitted to any
    /// regulatory authority outside the UK? Yes or No only — no Unknown option.
    /// </summary>
    public YesNo? HasIntlSubmission { get; set; }

    /// <summary>
    /// Conditional on HasIntlSubmission = Yes. CiC.
    /// Free text covering regulatory authorities or regions involved,
    /// whether submission has been made or is planned, and dates if known.
    /// </summary>
    public string? IntlSubmissionDetails { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
}
