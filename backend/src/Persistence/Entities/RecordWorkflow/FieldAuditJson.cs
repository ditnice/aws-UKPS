using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.RecordWorkflow;

/// <summary>
/// Serialises the JSON values stored in <see cref="RecordFieldAudit"/>, so every audited value
/// shares one format.
/// </summary>
internal static class FieldAuditJson
{
    // Audit values are stored and compared, never written into HTML, so characters such as "ö",
    // "+" and "'" are kept as they are rather than escaped, keeping the stored text readable and
    // directly queryable.
    private static readonly JsonSerializerOptions _options = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, _options);

    /// <summary>
    /// The audit value of a revision's names and identifiers: one JSON object keyed by
    /// <see cref="NameAndIdentifierType"/> member name in a fixed order, each type's names in
    /// display order, so the same collection always serialises the same way.
    /// </summary>
    public static string NamesAndIdentifiers(
        IEnumerable<string> genericNames,
        IEnumerable<string> otherIdentifiers
    ) => Serialize(new NamesAndIdentifiersValue([.. genericNames], [.. otherIdentifiers]));

    /// <summary>
    /// The audit value of a regulatory date: its precision, value and confidentiality, for example
    /// <c>{"date_precision":"EstimatedQuarter","date_value":"2027-04-01","is_confidential":true}</c>.
    /// </summary>
    public static string RegulatoryDate(
        DatePrecision precision,
        DateOnly value,
        bool isConfidential
    ) =>
        Serialize(
            new RegulatoryDateValue(
                precision.ToString(),
                value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                isConfidential
            )
        );

    private sealed record RegulatoryDateValue(
        [property: JsonPropertyName("date_precision")] string DatePrecision,
        [property: JsonPropertyName("date_value")] string DateValue,
        [property: JsonPropertyName("is_confidential")] bool IsConfidential
    );

    private sealed record NamesAndIdentifiersValue(
        [property: JsonPropertyName(nameof(NameAndIdentifierType.GenericName))]
            string[] GenericName,
        [property: JsonPropertyName(nameof(NameAndIdentifierType.OtherIdentifier))]
            string[] OtherIdentifier
    );
}
