using System.Collections;
using UKPS.Api.Application.RecordForms.Model;

namespace UKPS.Api.Application.RecordForms;

internal sealed class PageAnswers : IEnumerable<KeyValuePair<string, FormAnswer>>
{
    private readonly Dictionary<string, FormAnswer> _lookup;

    public PageAnswers(Dictionary<string, FormAnswer> lookup)
    {
        ArgumentNullException.ThrowIfNull(lookup);

        _lookup = new Dictionary<string, FormAnswer>(lookup, lookup.Comparer);
    }

    /// <summary>The answers of the page's questions, taken from a wider set of answers.</summary>
    public static PageAnswers For(FormPage page, IReadOnlyDictionary<string, FormAnswer> answers) =>
        new(
            page.Questions.Where(q => answers.ContainsKey(q.Id))
                .ToDictionary(static q => q.Id, q => answers[q.Id], StringComparer.Ordinal)
        );

    internal string ComputeStableHash() => AnswerHash.ForPage(_lookup);

    internal bool PageHashIsEqual(PageAnswers other)
    {
        return PageHashIsEqual(other.ComputeStableHash());
    }

    internal bool PageHashIsEqual(string pageHash)
    {
        var hash = ComputeStableHash();
        return string.Equals(pageHash, hash, StringComparison.Ordinal);
    }

    public FormAnswer this[string key] => _lookup[key];

    public Dictionary<string, FormAnswer>.Enumerator GetEnumerator() => _lookup.GetEnumerator();

    IEnumerator<KeyValuePair<string, FormAnswer>> IEnumerable<
        KeyValuePair<string, FormAnswer>
    >.GetEnumerator() => _lookup.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
