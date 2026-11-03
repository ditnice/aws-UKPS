using System.Text.Encodings.Web;
using System.Text.Json;

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
}
