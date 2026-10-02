namespace UKPS.Api.Application.Records.Dtos;

/// <summary>
/// Specifies the fields by which records can be sorted.
/// </summary>
public enum GetRecordsQuerySortValue
{
    /// <summary>
    /// Sorts records by the next update due date.
    /// </summary>
    NextUpdateDue = 0,

    /// <summary>
    /// Sorts records by their identifier.
    /// </summary>
    Id = 1,

    /// <summary>
    /// Sorts records by their development name.
    /// </summary>
    DevelopmentName = 2,

    /// <summary>
    /// Sorts records by their record status.
    /// </summary>
    RecordStatus = 3,
}
