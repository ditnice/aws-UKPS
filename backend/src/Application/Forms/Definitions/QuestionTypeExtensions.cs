using System.Text.Json.Nodes;

namespace UKPS.Api.Application.Forms.Definitions;

internal static class QuestionTypeExtensions
{
    /// <summary>
    /// Whether the question's answer is a list of option values rather than a single value.
    /// </summary>
    public static bool IsMultiValue(this QuestionType type) => type == QuestionType.Checkbox;

    public static bool HasOptions(this QuestionType type) => type != QuestionType.Textarea;

    /// <summary>
    /// Checks the JSON shape of a submitted answer: <c>string | null</c> for single-value
    /// questions, <c>string[]</c> (no nulls, no duplicates) for multi-value questions.
    /// </summary>
    public static bool IsValidShape(this QuestionType type, JsonNode? value)
    {
        if (type.IsMultiValue())
        {
            if (value is not JsonArray array)
            {
                return false;
            }

            var items = new HashSet<string>(StringComparer.Ordinal);
            return array.All(item => AnswerValues.TryGetString(item, out var s) && items.Add(s));
        }

        return value is null || AnswerValues.TryGetString(value, out _);
    }

    /// <summary>
    /// Normalises a shape-checked answer so equal answers compare equal: free text is trimmed
    /// and whitespace-only becomes <c>null</c>; multi-value answers are sorted (ordinal).
    /// </summary>
    public static JsonNode? Normalise(this QuestionType type, JsonNode? value)
    {
        if (type.IsMultiValue())
        {
            return AnswerValues.ToArray(
                AnswerValues.GetStrings(value).Order(StringComparer.Ordinal)
            );
        }

        if (type != QuestionType.Textarea || !AnswerValues.TryGetString(value, out var text))
        {
            return value?.DeepClone();
        }

        var trimmed = text.Trim();
        return trimmed.Length == 0 ? null : JsonValue.Create(trimmed);
    }
}
