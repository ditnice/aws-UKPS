using UKPS.Api.Enums;

namespace UKPS.Api.Entities.SharedRevisionContent;

internal sealed class RecordGlobalSubmission
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public string? GlobalFirstSubmissionRegion { get; set; }
    public string? GlobalFirstSubmissionNotes { get; set; }
    public int? GlobalSubmissionEstimatedDateId { get; set; }
    public int? GlobalSubmissionActualDateId { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
    public RegulatoryDate? GlobalSubmissionEstimatedDate { get; set; }
    public RegulatoryDate? GlobalSubmissionActualDate { get; set; }
}
