namespace UKPS.Api.Application.Records.Dtos;

/// <summary>
/// Whether a record waiting in the QA worklist is being reviewed for the first time.
/// </summary>
public enum QaRecordType
{
    /// <summary>
    /// The record has never been published, so this is its first submission to QA.
    /// </summary>
    New = 0,

    /// <summary>
    /// The record has been published before and QA is reviewing changes made against it.
    /// </summary>
    Updated = 1,
}
