namespace UKPS.Api.Persistence.Enums;

/// <summary>
/// Represents the various statuses that a revision can have.
/// </summary>
public enum WorkflowStatus
{
    /// <summary>
    /// The revision is in the draft stage and has not been submitted for review.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// The revision is currently under review.
    /// </summary>
    QAReview = 1,

    /// <summary>
    /// The revision has been reviewed and published.
    /// </summary>
    Published = 2,

    /// <summary>
    /// The revision has been reviewed and rejected.
    /// </summary>
    Rejected = 3,
}
