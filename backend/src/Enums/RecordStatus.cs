namespace UKPS.Api.Enums;

/// <summary>
/// Represents the status of a record in the system.
/// </summary>
public enum RecordStatus
{
    /// <summary>
    /// The record is unpublished and not visible to users.
    /// </summary>
    Unpublished = 0,

    /// <summary>
    /// The record is active and available for use.
    /// </summary>
    Active = 1,

    /// <summary>
    /// The record is temporarily on hold and not currently in use.
    /// </summary>
    OnHold = 2,

    /// <summary>
    /// The record is archived and no longer active.
    /// </summary>
    Archived = 3,
}
