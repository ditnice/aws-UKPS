namespace UKPS.Api.Application.Records.Dtos;

/// <summary>
/// Identifies a status change a user can make to a record.
/// </summary>
public enum RecordStatusChangeAction
{
    /// <summary>
    /// Puts a published record on hold.
    /// </summary>
    PutOnHold = 0,

    /// <summary>
    /// Returns an on hold record to published.
    /// </summary>
    Resume = 1,

    /// <summary>
    /// Archives a published or on hold record.
    /// </summary>
    Archive = 2,
}
