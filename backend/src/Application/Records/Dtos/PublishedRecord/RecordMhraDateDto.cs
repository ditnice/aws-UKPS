namespace UKPS.Api.Application.Records.Dtos.PublishedRecord;

/// <summary>
/// Represents the MHRA dates section of a record.
/// </summary>
public sealed record RecordMhraDateDto
{
    /// <summary>
    /// Gets the UK submission date.
    /// </summary>
    public RegulatoryDateDto? UkSubmissionDate { get; init; }

    /// <summary>
    /// Gets the UK licence date.
    /// </summary>
    public RegulatoryDateDto? UkLicenceDate { get; init; }

    /// <summary>
    /// Gets the UK launch date.
    /// </summary>
    public RegulatoryDateDto? UkLaunchDate { get; init; }
}
