namespace UKPS.Api.Persistence.Enums;

/// <summary>
/// Represents the reasons for a change in the status of a record.
/// </summary>
public enum RecordStatusChangeReason
{
    /// <summary>
    /// Indicates that development has been discontinued.
    /// </summary>
    DevelopmentDiscontinued = 0,

    /// <summary>
    /// Indicates that the trial has been suspended.
    /// </summary>
    TrialSuspended = 1,

    /// <summary>
    /// Indicates that the filing has been withdrawn.
    /// </summary>
    FilingWithdrawn = 2,

    /// <summary>
    /// Indicates that the record is awaiting external clarification.
    /// </summary>
    AwaitingExternalClarification = 3,

    /// <summary>
    /// Indicates that a suspended trial has resumed. Only valid when moving
    /// from on_hold back to active.
    /// </summary>
    TrialResumed = 4,

    /// <summary>
    /// Indicates that withdrawn or clarification-pending plans have been
    /// refiled. Only valid when moving from on_hold back to active.
    /// </summary>
    PlansRefiled = 5,

    /// <summary>
    /// Indicates that the reason for the status change is other than the predefined reasons.
    /// </summary>
    Other = 6,
}
