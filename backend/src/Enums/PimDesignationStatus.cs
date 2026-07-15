namespace UKPS.Api.Enums;

/// <summary>
/// Represents the designation status in the PIM (Product Information Management) system.
/// </summary>
public enum PimDesignationStatus
{
    /// <summary>
    /// Indicates that the designation has been granted.
    /// </summary>
    Granted = 0,

    /// <summary>
    /// Indicates that the designation has not been granted.
    /// </summary>
    NotGranted = 1,

    /// <summary>
    /// Indicates that the decision on the designation is pending.
    /// </summary>
    DecisionPending = 2,
}
