using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace UKPS.Api.Application.Forms.Definitions;

/// <summary>
/// Helpers for answer values, which are held as wire JSON (<see cref="JsonNode"/>) so that
/// every question type shares one representation for payloads, diffs and audit.
/// </summary>
internal static class AnswerValues
{
    public static bool TryGetString(JsonNode? node, [NotNullWhen(true)] out string? value)
    {
        if (node is JsonValue jsonValue && jsonValue.GetValueKind() == JsonValueKind.String)
        {
            value = jsonValue.GetValue<string>();
            return true;
        }

        value = null;
        return false;
    }

    /// <summary>
    /// The option values in a multi-value answer. Assumes the shape has been checked.
    /// </summary>
    public static IReadOnlyList<string> GetStrings(JsonNode? node) =>
        node is JsonArray array ? [.. array.Select(item => item!.GetValue<string>())] : [];

    public static JsonArray ToArray(IEnumerable<string> values) =>
        new([.. values.Select(value => (JsonNode)JsonValue.Create(value))]);

    public static bool AreEqual(JsonNode? left, JsonNode? right) =>
        JsonNode.DeepEquals(left, right);

    /// <summary>
    /// The audit representation of an answer: scalars as the raw string (e.g. <c>Unknown</c>),
    /// anything else as wire JSON, and <c>null</c> for no answer.
    /// </summary>
    public static string? ToAuditValue(JsonNode? node) =>
        node switch
        {
            null => null,
            JsonArray { Count: 0 } => null,
            _ when TryGetString(node, out var text) => text,
            _ => node.ToJsonString(),
        };
}
