namespace UKPS.Api.Enums;

/// <summary>
/// Represents a tri-state enumeration for indicating a "Yes", "No", or "Not Yet Confirmed" status.
/// </summary>
public enum YesNoNotYetConfirmed
{
    /// <summary>
    /// Indicates a "Yes" response.
    /// </summary>
    Yes = 0,

    /// <summary>
    /// Indicates a "No" response.
    /// </summary>
    No = 1,

    /// <summary>
    /// Indicates that the response is "Not Yet Confirmed".
    /// </summary>
    NotYetConfirmed = 2,
}
