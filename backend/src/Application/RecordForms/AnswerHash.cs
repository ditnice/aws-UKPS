using System.Security.Cryptography;
using System.Text;
using UKPS.Api.Application.RecordForms.Model;

namespace UKPS.Api.Application.RecordForms;

/// <summary>
/// Hashes of answers, as upper-case hex SHA-256 of their canonical serialisation: answers in
/// ordinal key order, each with its shape and values.
/// </summary>
internal static class AnswerHash
{
    /// <summary>The hash a page save checks, so it only overwrites the answers the user saw.</summary>
    public static string ForPage(IReadOnlyDictionary<string, FormAnswer> answers) =>
        Compute(writer => Write(writer, answers));

    /// <summary>
    /// The hash Submit checks, so the record submitted is the one the user reviewed on check your
    /// answers. It covers the revision ID, so a new draft with the same answers still changes it,
    /// and every question in the form, including those that don't apply.
    /// </summary>
    public static string ForRevision(
        int revisionId,
        IReadOnlyDictionary<string, FormAnswer> answers
    ) =>
        Compute(writer =>
        {
            writer.Write(revisionId);
            Write(writer, answers);
        });

    private static string Compute(Action<BinaryWriter> write)
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            write(writer);
        }

        return Convert.ToHexString(SHA256.HashData(stream.ToArray()));
    }

    private static void Write(BinaryWriter writer, IReadOnlyDictionary<string, FormAnswer> answers)
    {
        writer.Write(answers.Count);

        foreach (var (key, answer) in answers.OrderBy(kv => kv.Key, StringComparer.Ordinal))
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
    }
}
