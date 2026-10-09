using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records.Dtos.RecordDetails;

/// <summary>
/// Represents the answers about a record's clinical trials as a whole.
/// </summary>
public sealed record RecordClinicalTrialInformationDto
{
    /// <summary>
    /// Gets whether any of the record's clinical trials are recruiting in the UK.
    /// </summary>
    public YesNoUnknown? RecruitingInUk { get; init; }
}
