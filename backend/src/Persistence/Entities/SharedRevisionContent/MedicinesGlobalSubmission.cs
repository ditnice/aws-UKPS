namespace UKPS.Api.Persistence.Entities.SharedRevisionContent;

internal sealed class MedicinesGlobalSubmission
{
    public int Id { get; set; }
    public int RevisionId { get; set; }

    /// <summary>
    /// Medicines only. Conditional on actual UK submission date being entered.
    /// The region or regulator responsible for the global first submission.
    /// </summary>
    public string? GlobalFirstSubmissionRegion { get; set; }

    /// <summary>
    /// Medicines only. Conditional on actual UK submission date being entered.
    /// </summary>
    public int? GlobalSubmissionActualDateId { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
    public RegulatoryDate? GlobalSubmissionActualDate { get; set; }
}
