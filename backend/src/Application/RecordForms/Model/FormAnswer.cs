using System.Text.Json;
using System.Text.Json.Nodes;
using UKPS.Api.Persistence.Entities.RecordWorkflow;

namespace UKPS.Api.Application.RecordForms.Model;

/// <summary>
/// A normalised answer: a single string (or null), a sorted set of strings, an ordered list of
/// strings, or an ordered list of group items. Two answers are equal when they would be stored the
/// same way, so a reordered multi-value selection is not a change, but a reordered list is.
/// </summary>
internal sealed class FormAnswer : IEquatable<FormAnswer>
{
    private static readonly IReadOnlyList<string> _noValues = [];
    private static readonly IReadOnlyList<FormGroupItem> _noItems = [];

    public AnswerShape Shape { get; }

    /// <summary>The answer of a single-value question; null when unanswered.</summary>
    public string? Value { get; }

    /// <summary>
    /// The answers of a multi-value question, sorted ordinally, or of a list question, in order.
    /// </summary>
    public IReadOnlyList<string> Values { get; }

    /// <summary>The items of a group question, in order.</summary>
    public IReadOnlyList<FormGroupItem> Items { get; }

    public bool IsEmpty =>
        Shape switch
        {
            AnswerShape.Single => Value is null,
            AnswerShape.Group => Items.Count == 0,
            _ => Values.Count == 0,
        };

    private FormAnswer(
        AnswerShape shape,
        string? value,
        IReadOnlyList<string> values,
        IReadOnlyList<FormGroupItem> items
    )
    {
        Shape = shape;
        Value = value;
        Values = values;
        Items = items;
    }

    public static FormAnswer Single(string? value) =>
        new(AnswerShape.Single, value, _noValues, _noItems);

    public static FormAnswer Multi(IEnumerable<string> values) =>
        new(AnswerShape.Multi, null, values.Order(StringComparer.Ordinal).ToArray(), _noItems);

    public static FormAnswer List(IEnumerable<string> values) =>
        new(AnswerShape.List, null, values.ToArray(), _noItems);

    public static FormAnswer Group(IEnumerable<FormGroupItem> items) =>
        new(AnswerShape.Group, null, _noValues, items.ToArray());

    public static FormAnswer Empty(AnswerShape shape) =>
        shape switch
        {
            AnswerShape.Single => Single(null),
            AnswerShape.Multi => Multi([]),
            AnswerShape.List => List([]),
            AnswerShape.Group => Group([]),
            _ => throw new InvalidOperationException($"Unsupported answer shape {shape}."),
        };

    /// <summary>Every option value in the answer, for a single or multi-value answer.</summary>
    public IEnumerable<string> AllValues() => Value is null ? Values : [Value];

    /// <summary>
    /// The audit representation: single values raw, lists and groups as JSON, no answer as null.
    /// </summary>
    public string? ToAuditText()
    {
        if (IsEmpty)
        {
            return null;
        }

        return Shape switch
        {
            AnswerShape.Single => Value,
            AnswerShape.Group => FieldAuditJson.Serialize(ToJsonNode()),
            _ => FieldAuditJson.Serialize(Values),
        };
    }

    public JsonElement ToJson() => JsonSerializer.SerializeToElement(ToJsonNode());

    /// <summary>The answer as it is sent on the wire.</summary>
    public JsonNode? ToJsonNode() =>
        Shape switch
        {
            AnswerShape.Single => Value is null ? null : JsonValue.Create(Value),
            AnswerShape.Group => new JsonArray([.. Items.Select(static i => i.ToJsonNode())]),
            _ => new JsonArray([.. Values.Select(static v => (JsonNode?)JsonValue.Create(v))]),
        };

    public bool Equals(FormAnswer? other)
    {
        if (other is null)
        {
            return false;
        }

        return Shape == other.Shape
            && string.Equals(Value, other.Value, StringComparison.Ordinal)
            && Values.SequenceEqual(other.Values, StringComparer.Ordinal)
            && Items.SequenceEqual(other.Items);
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

        foreach (FormGroupItem item in Items)
        {
            hash.Add(item);
        }

        return hash.ToHashCode();
    }
}
