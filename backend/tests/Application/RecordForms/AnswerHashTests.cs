using System.Security.Cryptography;
using System.Text;
using Shouldly;
using UKPS.Api.Application.RecordForms;
using UKPS.Api.Application.RecordForms.Model;

namespace UKPS.Api.Tests.Application.RecordForms;

public class AnswerHashTests
{
    private static readonly Dictionary<string, FormAnswer> _answers = new(StringComparer.Ordinal)
    {
        ["b.text"] = FormAnswer.Single("Text"),
        ["a.multi"] = FormAnswer.Multi(["2", "1"]),
        ["c.unanswered"] = FormAnswer.Single(null),
    };

    [Fact]
    public void ForRevision_SameRevisionAndAnswersInAnotherOrder_IsTheSame()
    {
        Dictionary<string, FormAnswer> reordered = _answers
            .Reverse()
            .ToDictionary(StringComparer.Ordinal);

        AnswerHash.ForRevision(1, reordered).ShouldBe(AnswerHash.ForRevision(1, _answers));
    }

    [Fact]
    public void ForRevision_AnotherRevisionWithTheSameAnswers_Differs()
    {
        AnswerHash.ForRevision(2, _answers).ShouldNotBe(AnswerHash.ForRevision(1, _answers));
    }

    [Fact]
    public void ForRevision_AnyAnswerChanged_Differs()
    {
        string hash = AnswerHash.ForRevision(1, _answers);

        AnswerHash.ForRevision(1, With("c.unanswered", FormAnswer.Single(""))).ShouldNotBe(hash);
        AnswerHash.ForRevision(1, With("a.multi", FormAnswer.Multi(["1"]))).ShouldNotBe(hash);
    }

    [Fact]
    public void ForPage_IsUnchangedBySharingTheSerialisation()
    {
        AnswerHash.ForPage(_answers).ShouldBe(OriginalPageHash(_answers));
    }

    private static Dictionary<string, FormAnswer> With(string questionId, FormAnswer answer) =>
        new(_answers, StringComparer.Ordinal) { [questionId] = answer };

    // The page hash as it was calculated before the revision hash shared its serialisation, so
    // pages loaded before a deploy can still be saved after it.
    private static string OriginalPageHash(Dictionary<string, FormAnswer> answers)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
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

        return Convert.ToHexString(SHA256.HashData(stream.ToArray()));
    }
}
