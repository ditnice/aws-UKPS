namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

/// <summary>Medicines only — the global first submission of this product.</summary>
internal sealed class MedicinesGlobalSubmission
{
    public int Id { get; set; }
    public int RevisionId { get; set; }

    /// <summary>
    /// Conditional on an actual UK submission date being entered.
    /// The region or regulator responsible for the global first submission.
    /// </summary>
    public string? GlobalFirstSubmissionRegion { get; set; }

    /// <summary>Conditional on an actual UK submission date being entered.</summary>
    public int? GlobalSubmissionActualDateId { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
    public SharedRevisionContent.RegulatoryDate? GlobalSubmissionActualDate { get; set; }
}
