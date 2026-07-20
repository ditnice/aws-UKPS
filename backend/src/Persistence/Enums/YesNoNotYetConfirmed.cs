namespace UKPS.Api.Persistence.Enums;

/// <summary>
/// Represents a tri-state enumeration for indicating a "Yes", "No", or "Not Yet Confirmed" status.
/// </summary>
public enum YesNoNotYetConfirmed
{
    /// <summary>
    /// Indicates that the response is "Not Yet Confirmed".
    /// </summary>
    NotYetConfirmed = 0,

    /// <summary>
    /// Indicates a "No" response.
    /// </summary>
    No = 1,

    /// <summary>
    /// Indicates a "Yes" response.
    /// </summary>
    Yes = 2,
}
