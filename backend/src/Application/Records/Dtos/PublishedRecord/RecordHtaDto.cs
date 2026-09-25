using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records.Dtos.PublishedRecord;

/// <summary>
/// Represents the health technology assessment section of a record.
/// </summary>
public sealed record RecordHtaDto
{
    /// <summary>
    /// Gets whether a medicine HTA submission is intended.
    /// </summary>
    public YesNoUnknown? MedicineHtaSubmissionIntended { get; init; }

    /// <summary>
    /// Gets the HTA bodies the medicine will be submitted to, or <c>null</c> if unanswered.
    /// </summary>
    public IReadOnlyCollection<MedicineHtaAssessor>? MedicineHtaBodies { get; init; }

    /// <summary>
    /// Gets whether the NICE aligned pathway applies.
    /// </summary>
    public YesNoUnknown? HtaNiceAlignedPathway { get; init; }

    /// <summary>
    /// Gets additional HTA details.
    /// </summary>
    public string? HtaAdditionalDetails { get; init; }
}
