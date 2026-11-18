using System.Text.Json.Nodes;

namespace UKPS.Api.Application.RecordForms.Model;

/// <summary>One item of a group answer: the answer of each child question, keyed by child ID.</summary>
internal sealed class FormGroupItem : IEquatable<FormGroupItem>
{
    public IReadOnlyDictionary<string, FormAnswer> Answers { get; }

    public FormGroupItem(IReadOnlyDictionary<string, FormAnswer> answers)
    {
        Answers = answers;
    }

    public FormAnswer this[string childId] => Answers[childId];

    /// <summary>The item as it is sent on the wire, with its children in ordinal key order.</summary>
    public JsonObject ToJsonNode() =>
        new(
            Answers
                .OrderBy(static a => a.Key, StringComparer.Ordinal)
                .Select(static a => KeyValuePair.Create(a.Key, a.Value.ToJsonNode()))
        );

    public bool Equals(FormGroupItem? other) =>
        other is not null
        && Answers.Count == other.Answers.Count
        && Answers.All(a =>
            other.Answers.TryGetValue(a.Key, out FormAnswer? answer) && a.Value.Equals(answer)
        );

    public override bool Equals(object? obj) => Equals(obj as FormGroupItem);

    public override int GetHashCode()
    {
        HashCode hash = new();
        foreach (var (key, answer) in Answers.OrderBy(static a => a.Key, StringComparer.Ordinal))
        {
            hash.Add(key, StringComparer.Ordinal);
            hash.Add(answer);
        }

        return hash.ToHashCode();
    }
}
