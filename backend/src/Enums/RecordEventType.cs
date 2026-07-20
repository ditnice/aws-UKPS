namespace UKPS.Api.Enums;

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
    /// Indicates that the status of a record has changed.
    /// </summary>
    RecordStatusChanged = 1,

    /// <summary>
    /// Indicates that a draft has been created.
    /// </summary>
    DraftCreated = 2,

    /// <summary>
    /// Indicates that a draft has been saved.
    /// </summary>
    DraftSaved = 3,

    /// <summary>
    /// Indicates that a draft has been submitted.
    /// </summary>
    DraftSubmitted = 4,

    /// <summary>
    /// Indicates that a draft has been superseded.
    /// </summary>
    DraftSuperseded = 5,

    /// <summary>
    /// Indicates that a QA review has been created.
    /// </summary>
    QaReviewCreated = 6,

    /// <summary>
    /// Indicates that a QA review has been completed.
    /// </summary>
    QaReviewCompleted = 7,

    /// <summary>
    /// Indicates that a QA issue has been added.
    /// </summary>
    QaIssueAdded = 8,

    /// <summary>
    /// Indicates that a QA issue has been resolved.
    /// </summary>
    QaIssueResolved = 9,

    /// <summary>
    /// Indicates that a QA issue has been reopened.
    /// </summary>
    QaIssueReopened = 10,

    /// <summary>
    /// Indicates that a revision has been rejected.
    /// </summary>
    RevisionRejected = 11,

    /// <summary>
    /// Indicates that a revision has been published.
    /// </summary>
    RevisionPublished = 12,

    /// <summary>
    /// Indicates that a record has been reviewed with no changes.
    /// </summary>
    RecordReviewedNoChange = 13,
}
