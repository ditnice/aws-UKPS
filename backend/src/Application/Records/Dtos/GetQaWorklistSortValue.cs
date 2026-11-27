namespace UKPS.Api.Application.Records.Dtos;

/// <summary>
/// Specifies the fields by which the QA worklist can be sorted.
/// </summary>
public enum GetQaWorklistSortValue
{
    /// <summary>
    /// Sorts records by their identifier.
    /// </summary>
    Id = 0,

    /// <summary>
    /// Sorts records by the name of the organisation that owns them.
    /// </summary>
    Organisation = 1,

    /// <summary>
    /// Sorts records by their generic names.
    /// </summary>
    GenericNames = 2,

    /// <summary>
    /// Sorts records by when they were submitted to QA.
    /// </summary>
    SubmittedToQa = 3,

    /// <summary>
    /// Sorts records by whether they are new or updated.
    /// </summary>
    QaRecordType = 4,
}
