using System.Text.Json.Nodes;
using UKPS.Api.Application.Forms.Bindings;
using UKPS.Api.Application.Forms.Definitions;

namespace UKPS.Api.Application.Forms;

/// <summary>Reads stored answers for a set of questions.</summary>
internal static class AnswerReader
{
    /// <summary>
    /// Reads the stored answer for each question, keyed by question ID. Bindings share
    /// <paramref name="context"/>, so each content row is loaded once.
    /// </summary>
    public static async Task<Dictionary<string, JsonNode?>> ReadAsync(
        BindingContext context,
        IEnumerable<QuestionDefinition> questions,
        CancellationToken cancellationToken
    )
    {
        var answers = new Dictionary<string, JsonNode?>(StringComparer.Ordinal);

        // Sequential: bindings share one DbContext.
        foreach (var question in questions)
        {
            answers[question.Id] = await question.Binding.ReadAsync(context, cancellationToken);
        }

        return answers;
    }

    /// <summary>The option values in an answer, for looking up saved (possibly archived) options.</summary>
    public static IReadOnlyCollection<string> SelectedValues(JsonNode? answer) =>
        answer switch
        {
            JsonArray => AnswerValues.GetStrings(answer),
            _ when AnswerValues.TryGetString(answer, out var value) => [value],
            _ => [],
        };
}
