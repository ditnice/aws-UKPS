using System.Text.Json.Nodes;
using UKPS.Api.Application.Forms.Definitions;

namespace UKPS.Api.Application.Forms.Rules;

internal sealed record RequiredRule(string Message) : QuestionRule(Message)
{
    private static readonly HashSet<QuestionType> _appliesTo =
    [
        QuestionType.Textarea,
        QuestionType.Radio,
        QuestionType.Checkbox,
        QuestionType.Select,
    ];

    public override RuleKind Kind => RuleKind.Required;

    public override IReadOnlySet<QuestionType> AppliesTo => _appliesTo;

    public override bool IsSatisfiedBy(JsonNode? value) =>
        value switch
        {
            null => false,
            JsonArray array => array.Count > 0,
            _ => AnswerValues.TryGetString(value, out var text) && text.Trim().Length > 0,
        };
}
