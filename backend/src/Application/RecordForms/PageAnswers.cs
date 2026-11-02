using System.Collections;
using System.Security.Cryptography;
using System.Text;
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

    internal string ComputeStableHash()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

        writer.Write(_lookup.Count);

        foreach (var (key, answer) in _lookup.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            writer.Write(key);
            writer.Write((byte)answer.Shape);

            if (answer.Shape == AnswerShape.Single)
            {
                writer.Write(answer.Value is not null);
                if (answer.Value is not null)
                {
                    writer.Write(answer.Value);
                }
            }
            else
            {
                writer.Write(answer.Values.Count);
                foreach (string value in answer.Values)
                {
                    writer.Write(value);
                }
            }
        }

        return Convert.ToHexString(SHA256.HashData(stream.ToArray()));
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
