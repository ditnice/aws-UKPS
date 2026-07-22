namespace UKPS.Api.Persistence.Enums;

/// <summary>
/// Represents the various statuses that a workflow can have.
/// </summary>
public enum WorkflowStatus
{
    /// <summary>
    /// The workflow is in the draft stage and has not been submitted for review.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// The workflow is currently under review.
    /// </summary>
    InReview = 1,

    /// <summary>
    /// The workflow has been reviewed and published.
    /// </summary>
    Published = 2,

    /// <summary>
    /// The workflow has been reviewed and rejected.
    /// </summary>
    Rejected = 3,

    /// <summary>
    /// The workflow has been superseded by a newer version.
    /// </summary>
    Superseded = 4,
}
