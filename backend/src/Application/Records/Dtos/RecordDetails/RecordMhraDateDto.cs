using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records.Dtos.RecordDetails;

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
    /// Gets whether conditional approval is anticipated in the UK.
    /// </summary>
    public YesNoUnknown? UkConditionalApprovalAnticipated { get; init; }
}
