namespace UKPS.Api.Application.Records.Dtos.PublishedRecord;

/// <summary>
/// Represents the treatment detail section of a medicine record.
/// </summary>
public sealed record MedicinesTreatmentDetailDto
{
    /// <summary>
    /// Gets the proposed place in therapy.
    /// </summary>
    public required string ProposedPlaceInTherapy { get; init; }

    /// <summary>
    /// Gets the estimated duration of treatment.
    /// </summary>
    public string? EstimatedDurationOfTreatment { get; init; }
}
