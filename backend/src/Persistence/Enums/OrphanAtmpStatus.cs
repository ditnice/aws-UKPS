namespace UKPS.Api.Persistence.Enums;

/// <summary>
/// Represents the status of an orphan attempt in the system.
/// </summary>
public enum OrphanAtmpStatus
{
    /// <summary>
    /// Indicates that the decision to submit is ongoing.
    /// </summary>
    DecisionToSubmitOngoing = 0,

    /// <summary>
    /// Indicates that the application has been submitted.
    /// </summary>
    ApplicationSubmitted = 1,

    /// <summary>
    /// Indicates that the attempt has not been granted.
    /// </summary>
    No = 2,

    /// <summary>
    /// Indicates that the attempt has been granted.
    /// </summary>
    Granted = 3,
}
