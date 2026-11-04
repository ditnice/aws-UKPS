using System.Text.Json.Serialization;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records.Dtos.RecordDetails;

/// <summary>
/// Represents the data held on the latest revision of a record, whatever its workflow status.
/// The <c>recordType</c> property identifies the concrete type.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "recordType")]
[JsonDerivedType(typeof(MedicineRecordDto), nameof(RecordType.Medicine))]
[JsonDerivedType(typeof(VaccineRecordDto), nameof(RecordType.Vaccine))]
public abstract record RecordDto
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
    /// Gets the status shown to users for the record.
    /// </summary>
    public required RecordDisplayStatus DisplayStatus { get; init; }

    /// <summary>
    /// Gets the date the record was last reviewed, when available.
    /// </summary>
    public DateTime? ReviewedAt { get; init; }

    /// <summary>
    /// Gets whether the record type has a content form and the caller can edit it. When true,
    /// Update record links to the record's task list, which edits the latest revision.
    /// </summary>
    public required bool CanEditContent { get; init; }

    /// <summary>
    /// Gets the actions the user can take on the record, given its status and the workflow status
    /// of its latest revision.
    /// </summary>
    public required IReadOnlyCollection<RecordAction> Actions { get; init; }
}
