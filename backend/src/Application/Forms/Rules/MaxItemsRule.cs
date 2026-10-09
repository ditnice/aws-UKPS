using System.Text.Json.Nodes;
using UKPS.Api.Application.Forms.Definitions;

namespace UKPS.Api.Application.Forms.Rules;

internal sealed record MaxItemsRule(int Max, string Message) : QuestionRule(Message)
{
    private static readonly HashSet<QuestionType> _appliesTo = [QuestionType.Checkbox];

    public override string Kind => "maxItems";

    public override int? Value => Max;

    public override IReadOnlySet<QuestionType> AppliesTo => _appliesTo;

    public override bool IsSatisfiedBy(JsonNode? value) =>
        value is not JsonArray array || array.Count <= Max;
}
