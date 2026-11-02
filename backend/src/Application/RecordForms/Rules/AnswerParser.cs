using System.Text.Json;
using UKPS.Api.Application.RecordForms.Model;

namespace UKPS.Api.Application.RecordForms.Rules;

/// <summary>
/// Checks the JSON shape of a submitted answer and normalises it: text is trimmed and
/// whitespace-only becomes null; lists are sorted.
/// </summary>
internal static class AnswerParser
{
    /// <summary>Parses the answer, or returns the message describing why it is malformed.</summary>
    public static (FormAnswer? Answer, string? Error) Parse(AnswerShape shape, JsonElement element)
    {
        return shape == AnswerShape.Single ? ParseSingle(element) : ParseMulti(element);
    }

    private static (FormAnswer? Answer, string? Error) ParseSingle(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Null => (FormAnswer.Single(null), null),
            JsonValueKind.String => (FormAnswer.Single(Normalise(element.GetString())), null),
            _ => (null, RecordFormMessages.WrongShape),
        };
    }

    private static (FormAnswer? Answer, string? Error) ParseMulti(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Array)
        {
            return (null, RecordFormMessages.WrongShape);
        }

        List<string> values = [];
        foreach (JsonElement item in element.EnumerateArray())
        {
            string? value =
                item.ValueKind == JsonValueKind.String ? Normalise(item.GetString()) : null;
            if (value is null)
            {
                return (null, RecordFormMessages.WrongShape);
            }

            values.Add(value);
        }

        if (values.Distinct(StringComparer.Ordinal).Count() != values.Count)
        {
            return (null, RecordFormMessages.Duplicate);
        }

        return (FormAnswer.Multi(values), null);
    }

    private static string? Normalise(string? text)
    {
        string? trimmed = text?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }
}
