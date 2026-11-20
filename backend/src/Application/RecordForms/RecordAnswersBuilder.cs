using UKPS.Api.Application.RecordForms.Conditions;
using UKPS.Api.Application.RecordForms.Dtos;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Options;

namespace UKPS.Api.Application.RecordForms;

/// <summary>
/// Builds the summary rows of the questions that apply on a revision, leaving out tasks and
/// sections with none. Shared by check your answers and viewing a record.
/// </summary>
internal sealed class RecordAnswersBuilder(
    FormModel model,
    IReadOnlyDictionary<string, FormAnswer> answers,
    Applicability applicability,
    IReadOnlyDictionary<string, IReadOnlyList<FormOption>> options
)
{
    public IReadOnlyList<RecordAnswersSectionDto> Build() =>
        [.. model.Sections.Select(ToSection).Where(static section => section.Tasks.Count > 0)];

    private RecordAnswersSectionDto ToSection(FormSection section) =>
        new()
        {
            Id = section.Id,
            Title = section.Title,
            Tasks =
            [
                .. section.Tasks.Select(ToTask).Where(static task => task.Questions.Count > 0),
            ],
        };

    private RecordAnswersTaskDto ToTask(FormTask task) =>
        new()
        {
            Id = task.Id,
            Title = task.Title,
            Questions =
            [
                .. task
                    .Questions.Where(applicability.Applies)
                    .Select(question =>
                        ToQuestion(
                            question,
                            model.PageOf(question),
                            answers[question.Id],
                            options.GetValueOrDefault(question.Id)
                        )
                    ),
            ],
        };

    private RecordAnswersQuestionDto ToQuestion(
        FormQuestion question,
        FormPage page,
        FormAnswer answer,
        IReadOnlyList<FormOption>? questionOptions
    )
    {
        bool isGroup = question.Shape == AnswerShape.Group;
        return new()
        {
            Id = question.Id,
            PageId = page.Id,
            Label = question.Label,
            Type = question.Control,
            Multiple = question.Shape == AnswerShape.Multi,
            Values = [.. answer.AllValues()],
            SelectedOptions = questionOptions is null
                ? null
                : SelectedOptions(answer, questionOptions),
            ItemLabel = isGroup ? question.ItemLabel : null,
            AddAnotherLabel = isGroup ? question.AddAnotherLabel : null,
            Items = isGroup
                ? [.. answer.Items.Select(item => ToGroupItem(question, page, item))]
                : null,
        };
    }

    /// <summary>
    /// The answer to each of a group's questions in one item. Options are only loaded for a page's
    /// own questions, so these show their raw values; none of them is a choice question yet. Their
    /// IDs are local to the group.
    /// </summary>
    private RecordAnswersGroupItemDto ToGroupItem(
        FormQuestion group,
        FormPage page,
        FormGroupItem item
    ) =>
        new()
        {
            Questions =
            [
                .. group.Children.Select(child =>
                    ToQuestion(child, page, item[child.Id], questionOptions: null)
                ),
            ],
        };

    /// <summary>
    /// The options the answer chose, in the order offered. A value that is not offered is left
    /// out; check your answers reports it as an error.
    /// </summary>
    /// <remarks>
    /// Options are at most two levels deep, which the tree option provider enforces, so a chosen
    /// option is either a parent or one of its children.
    /// </remarks>
    private static RecordAnswersOptionDto[] SelectedOptions(
        FormAnswer answer,
        IReadOnlyList<FormOption> options
    )
    {
        HashSet<string> chosen = answer.AllValues().ToHashSet(StringComparer.Ordinal);
        return
        [
            .. options.SelectMany(parent =>
                parent
                    .SelfAndChildren()
                    .Where(option => chosen.Contains(option.Value))
                    .Select(option => new RecordAnswersOptionDto
                    {
                        Value = option.Value,
                        Label = option.Label,
                        ParentLabel = ReferenceEquals(option, parent) ? null : parent.Label,
                    })
            ),
        ];
    }
}
