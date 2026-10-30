using System.Text.Json;

namespace UKPS.Api.Application.RecordForms.Model;

/// <summary>
/// A normalised answer: a single string (or null), or a sorted list of strings. Two answers are
/// equal when they would be stored the same way, so a reordered selection is not a change.
/// </summary>
internal sealed class FormAnswer : IEquatable<FormAnswer>
{
    private static readonly IReadOnlyList<string> _noValues = [];

    public AnswerShape Shape { get; }

    /// <summary>The answer of a single-value question; null when unanswered.</summary>
    public string? Value { get; }

    /// <summary>The answers of a multi-value question, sorted ordinally.</summary>
    public IReadOnlyList<string> Values { get; }

    public bool IsEmpty => Shape == AnswerShape.Single ? Value is null : Values.Count == 0;

    private FormAnswer(AnswerShape shape, string? value, IReadOnlyList<string> values)
    {
        Shape = shape;
        Value = value;
        Values = values;
    }

    public static FormAnswer Single(string? value) => new(AnswerShape.Single, value, _noValues);

    public static FormAnswer Multi(IEnumerable<string> values) =>
        new(AnswerShape.Multi, null, values.Order(StringComparer.Ordinal).ToArray());

    public static FormAnswer Empty(AnswerShape shape) =>
        shape == AnswerShape.Single ? Single(null) : Multi([]);

    /// <summary>Every value in the answer, whatever its shape.</summary>
    public IEnumerable<string> AllValues() => Value is null ? Values : [Value];

    /// <summary>
    /// The audit representation: single values raw, lists as a JSON array, no answer as null.
    /// </summary>
    public string? ToAuditText()
    {
        if (IsEmpty)
        {
            return null;
        }

        return Shape == AnswerShape.Single ? Value : JsonSerializer.Serialize(Values);
    }

    public JsonElement ToJson() =>
        Shape == AnswerShape.Single
            ? JsonSerializer.SerializeToElement(Value)
            : JsonSerializer.SerializeToElement(Values);

    public bool Equals(FormAnswer? other)
    {
        if (other is null)
        {
            return false;
        }

        return Shape == other.Shape
            && string.Equals(Value, other.Value, StringComparison.Ordinal)
            && Values.SequenceEqual(other.Values, StringComparer.Ordinal);
    }

    public override bool Equals(object? obj) => Equals(obj as FormAnswer);

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(Shape);
        hash.Add(Value, StringComparer.Ordinal);
        foreach (string value in Values)
        {
            hash.Add(value, StringComparer.Ordinal);
        }

        return hash.ToHashCode();
    }
}
