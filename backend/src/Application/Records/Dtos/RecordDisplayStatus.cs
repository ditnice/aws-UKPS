namespace UKPS.Api.Application.Records.Dtos;

/// <summary>
/// Represents the status shown to users for a record. On hold and archived records show their
/// record status; otherwise the workflow status of the latest revision is shown.
/// </summary>
public enum RecordDisplayStatus
{
    /// <summary>
    /// The latest revision is a draft, or was rejected and returned to the user to edit.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// The latest revision is under QA review.
    /// </summary>
    QAReview = 1,

    /// <summary>
    /// The latest revision is published.
    /// </summary>
    Published = 2,

    /// <summary>
    /// The record is on hold.
    /// </summary>
    OnHold = 3,

    /// <summary>
    /// The record is archived.
    /// </summary>
    Archived = 4,
}
