using System.Text.Json;
using UKPS.Api.Application.RecordForms.Model;

namespace UKPS.Api.Application.RecordForms.Rules;

/// <summary>
/// Checks the JSON shape of a submitted answer and normalises it: text is trimmed and
/// whitespace-only becomes null; multi-value answers are sorted; blank text list entries are
/// dropped.
/// </summary>
internal static class AnswerParser
{
    /// <summary>Parses the answer, or returns the message describing why it is malformed.</summary>
    public static (FormAnswer? Answer, string? Error) Parse(
        FormQuestion question,
        JsonElement element
    ) =>
        question.Shape switch
        {
            AnswerShape.Single => ParseSingle(element),
            AnswerShape.Multi => ParseMulti(element),
            AnswerShape.List => ParseList(element),
            _ => (null, RecordFormMessages.WrongShape),
        };

    /// <summary>Parses an answer of a single, multi-value or list shape.</summary>
    public static (FormAnswer? Answer, string? Error) Parse(
        AnswerShape shape,
        JsonElement element
    ) =>
        shape switch
        {
            AnswerShape.Single => ParseSingle(element),
            AnswerShape.Multi => ParseMulti(element),
            AnswerShape.List => ParseList(element),
            _ => throw new ArgumentOutOfRangeException(nameof(shape), shape, null),
        };

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

    /// <summary>An ordered list of strings; null and blank entries are dropped.</summary>
    private static (FormAnswer? Answer, string? Error) ParseList(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Array)
        {
            return (null, RecordFormMessages.WrongShape);
        }

        List<string> values = [];
        foreach (JsonElement item in element.EnumerateArray())
        {
            switch (item.ValueKind)
            {
                case JsonValueKind.Null:
                    continue;
                case JsonValueKind.String:
                    if (Normalise(item.GetString()) is { } value)
                    {
                        values.Add(value);
                    }

                    continue;
                default:
                    return (null, RecordFormMessages.WrongShape);
            }
        }

        return (FormAnswer.List(values), null);
    }

    private static string? Normalise(string? text)
    {
        string? trimmed = text?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }
}
