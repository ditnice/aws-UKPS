namespace UKPS.Api.Application.Records.Dtos;

/// <summary>
/// A record waiting for QA review, as shown in a row of the QA worklist.
/// </summary>
public sealed record QaWorklistItemDto
{
    /// <summary>
    /// Gets the record identifier.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the name of the organisation that owns the record.
    /// </summary>
    public required string OrganisationName { get; init; }

    /// <summary>
    /// Gets the generic names of the medicine.
    /// </summary>
    public required IReadOnlyCollection<string> GenericNames { get; init; }

    /// <summary>
    /// Gets when the latest revision was submitted to QA.
    /// </summary>
    public required DateTime SubmittedToQaAt { get; init; }

    /// <summary>
    /// Gets whether this is a first submission or a change to a published record.
    /// </summary>
    public required QaRecordType QaRecordType { get; init; }
}
