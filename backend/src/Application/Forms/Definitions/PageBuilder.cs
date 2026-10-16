using UKPS.Api.Application.Forms.Bindings;
using UKPS.Api.Application.Forms.Options;

namespace UKPS.Api.Application.Forms.Definitions;

internal sealed class PageBuilder
{
    public List<QuestionDefinition> Questions { get; } = [];

    public PageBuilder Textarea(
        string id,
        string label,
        IQuestionBinding binding,
        Action<QuestionBuilder>? configure = null
    ) => Add(id, QuestionType.Textarea, label, binding, options: null, configure);

    public PageBuilder Radio(
        string id,
        string label,
        IQuestionBinding binding,
        QuestionOptions options,
        Action<QuestionBuilder>? configure = null
    ) => Add(id, QuestionType.Radio, label, binding, options, configure);

    public PageBuilder Select(
        string id,
        string label,
        IQuestionBinding binding,
        QuestionOptions options,
        Action<QuestionBuilder>? configure = null
    ) => Add(id, QuestionType.Select, label, binding, options, configure);

    public PageBuilder Checkbox(
        string id,
        string label,
        IQuestionBinding binding,
        QuestionOptions options,
        Action<QuestionBuilder>? configure = null
    ) => Add(id, QuestionType.Checkbox, label, binding, options, configure);

    private PageBuilder Add(
        string id,
        QuestionType type,
        string label,
        IQuestionBinding binding,
        QuestionOptions? options,
        Action<QuestionBuilder>? configure
    )
    {
        var builder = new QuestionBuilder();
        configure?.Invoke(builder);
        Questions.Add(
            new QuestionDefinition(
                id,
                type,
                label,
                builder.HintText,
                builder.DisplayHint,
                builder.Rules,
                options,
                binding
            )
        );
        return this;
    }
}
