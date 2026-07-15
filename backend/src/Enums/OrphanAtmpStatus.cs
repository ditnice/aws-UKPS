namespace UKPS.Api.Enums;

/// <summary>
/// Represents the status of an orphan attempt in the system.
/// </summary>
public enum OrphanAtmpStatus
{
    /// <summary>
    /// Indicates that the attempt has been granted.
    /// </summary>
    Granted = 0,

    /// <summary>
    /// Indicates that the attempt has not been granted.
    /// </summary>
    No = 1,

    /// <summary>
    /// Indicates that the decision to submit is ongoing.
    /// </summary>
    DecisionToSubmitOngoing = 2,

    /// <summary>
    /// Indicates that the application has been submitted.
    /// </summary>
    ApplicationSubmitted = 3,
}
