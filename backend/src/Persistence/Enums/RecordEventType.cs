namespace UKPS.Api.Persistence.Enums;

/// <summary>
/// Represents the types of events that can occur for a record in the system.
/// </summary>
public enum RecordEventType
{
    /// <summary>
    /// Indicates that a record has been created.
    /// </summary>
    RecordCreated = 0,

    /// <summary>
    /// Indicates that a new revision has been created, either as the
    /// record's first draft or branched from an earlier revision.
    /// </summary>
    RevisionCreated = 1,

    /// <summary>
    /// Indicates that a revision has been submitted for QA review.
    /// </summary>
    SubmittedToQa = 2,

    /// <summary>
    /// Indicates that a revision has been rejected by QA.
    /// </summary>
    RevisionRejected = 3,

    /// <summary>
    /// Indicates that a revision has been published, either following QA
    /// approval or, for vaccine records, immediately on submission.
    /// </summary>
    RecordPublished = 4,

    /// <summary>
    /// Indicates that a record has been reviewed with no changes.
    /// </summary>
    RecordReviewedNoChange = 5,

    /// <summary>
    /// Indicates that the status of a record has changed.
    /// </summary>
    RecordStatusChanged = 6,
}
