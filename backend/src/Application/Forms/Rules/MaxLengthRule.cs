using System.Text.Json.Nodes;
using UKPS.Api.Application.Forms.Definitions;

namespace UKPS.Api.Application.Forms.Rules;

internal sealed record MaxLengthRule(int Max, string Message) : QuestionRule(Message)
{
    private static readonly HashSet<QuestionType> _appliesTo = [QuestionType.Textarea];

    public override string Kind => "maxLength";

    public override int? Value => Max;

    public override IReadOnlySet<QuestionType> AppliesTo => _appliesTo;

    // string.Length counts UTF-16 code units, matching JavaScript's String.length.
    public override bool IsSatisfiedBy(JsonNode? value) =>
        !AnswerValues.TryGetString(value, out var text) || text.Length <= Max;
}
