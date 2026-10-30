using System.Text.Json;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.SharedRevisionContent;

internal sealed class RecordNameAndIdentifier
{
    private static readonly JsonSerializerOptions _auditJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public int Id { get; set; }
    public int RecordProductDetailId { get; set; }
    public required string Name { get; set; }
    public NameAndIdentifierType NameType { get; set; }
    public int? DisplayOrder { get; set; }

    // Navigation
    public RecordProductDetail? RecordProductDetail { get; set; }

    /// <summary>
    /// The field audit value of a revision's names and identifiers: one JSON object keyed by type
    /// in a fixed order, each type's names in display order, so the same collection always
    /// serialises the same way.
    /// </summary>
    public static string AuditText(
        IEnumerable<string> genericNames,
        IEnumerable<string> otherIdentifiers
    ) =>
        JsonSerializer.Serialize(
            new AuditValue([.. genericNames], [.. otherIdentifiers]),
            _auditJsonOptions
        );

    private sealed record AuditValue(string[] GenericName, string[] OtherIdentifier);
}
