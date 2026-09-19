namespace UKPS.Api.Application.Records.Dtos;

/// <summary>
/// Represents the update status filter for records.
/// </summary>
public enum UpdateStatus
{
    /// <summary>
    /// Records whose next update due date has passed.
    /// </summary>
    Overdue = 0,

    /// <summary>
    /// Records whose next update due date has not yet passed, or has no due date.
    /// </summary>
    NotOverdue = 1,
}
