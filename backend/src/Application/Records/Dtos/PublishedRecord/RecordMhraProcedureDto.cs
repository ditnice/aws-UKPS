namespace UKPS.Api.Application.Records.Dtos.PublishedRecord;

/// <summary>
/// Represents the MHRA procedure section of a record.
/// </summary>
public sealed record RecordMhraProcedureDto
{
    /// <summary>
    /// Gets the MHRA procedure type.
    /// </summary>
    public ReferenceDataDto? MhraProcedureType { get; init; }

    /// <summary>
    /// Gets the IRP reference regulator. Populated when the procedure type is IRP.
    /// </summary>
    public ReferenceDataDto? IrpReferenceRegulator { get; init; }

    /// <summary>
    /// Gets the procedure details.
    /// </summary>
    public string? ProcedureDetails { get; init; }
}
