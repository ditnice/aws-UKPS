namespace UKPS.Api.Persistence.Enums;

/// <summary>
/// Defines the actions a user can take on a record.
/// </summary>
public enum RecordAction
{
    /// <summary>
    /// Edits the latest draft revision, or creates a new draft revision to edit.
    /// </summary>
    Update = 0,

    /// <summary>
    /// Puts the record on hold, resumes it or archives it.
    /// </summary>
    ChangeStatus = 1,

    /// <summary>
    /// Confirms the record's information is current.
    /// </summary>
    ConfirmCurrent = 2,

    /// <summary>
    /// Confirms the record is still on hold.
    /// </summary>
    ConfirmOnHold = 3,

    /// <summary>
    /// Discards a record that has never been submitted.
    /// </summary>
    Discard = 4,

    /// <summary>
    /// Views the record's timeline.
    /// </summary>
    ViewTimeline = 5,
}
