using UKPS.Api.Application.Forms.Rules;

namespace UKPS.Api.Application.Forms.Definitions;

internal sealed class QuestionBuilder
{
    public string? HintText { get; private set; }

    public string? DisplayHint { get; private set; }

    public List<QuestionRule> Rules { get; } = [];

    public QuestionBuilder Hint(string hint)
    {
        HintText = hint;
        return this;
    }

    public QuestionBuilder Display(string display)
    {
        DisplayHint = display;
        return this;
    }

    public QuestionBuilder Required(string message) => AddRule(new RequiredRule(message));

    public QuestionBuilder MaxLength(int max, string message) =>
        AddRule(new MaxLengthRule(max, message));

    public QuestionBuilder MaxItems(int max, string message) =>
        AddRule(new MaxItemsRule(max, message));

    private QuestionBuilder AddRule(QuestionRule rule)
    {
        Rules.Add(rule);
        return this;
    }
}
