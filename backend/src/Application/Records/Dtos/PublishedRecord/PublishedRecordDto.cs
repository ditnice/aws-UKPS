using System.Text.Json.Serialization;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records.Dtos.PublishedRecord;

/// <summary>
/// Represents the data held on the latest published revision of a record. The
/// <c>recordType</c> property identifies the concrete type.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "recordType")]
[JsonDerivedType(typeof(PublishedMedicineRecordDto), nameof(RecordType.Medicine))]
[JsonDerivedType(typeof(PublishedVaccineRecordDto), nameof(RecordType.Vaccine))]
public abstract record PublishedRecordDto
{
    /// <summary>
    /// Gets the record identifier.
    /// </summary>
    public required int RecordId { get; init; }

    /// <summary>
    /// Gets the identifier of the organisation that owns the record.
    /// </summary>
    public required int OrganisationId { get; init; }

    /// <summary>
    /// Gets the record status.
    /// </summary>
    public required RecordStatus RecordStatus { get; init; }

    /// <summary>
    /// Gets the date the record was last reviewed, when available.
    /// </summary>
    public DateTime? ReviewedAt { get; init; }

    /// <summary>
    /// Gets the identifier of the published revision.
    /// </summary>
    public required int RevisionId { get; init; }
}
