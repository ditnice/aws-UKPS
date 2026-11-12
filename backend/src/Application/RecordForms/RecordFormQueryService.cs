using UKPS.Api.Application.RecordForms.Conditions;
using UKPS.Api.Application.RecordForms.Dtos;
using UKPS.Api.Application.RecordForms.Errors;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Options;

namespace UKPS.Api.Application.RecordForms;

internal sealed class RecordFormQueryService(RecordFormPageLoader loader) : IRecordFormQueryService
{
    public async Task<GetRecordPageResult> GetPage(
        int recordId,
        string pageId,
        CancellationToken cancellationToken
    )
    {
        var (loaded, failure) = await loader.LoadPageAsync(
            recordId,
            pageId,
            track: false,
            cancellationToken
        );
        if (loaded is null)
        {
            return GetRecordPageResult.Err(
                failure == PageLookupFailure.NotAllowed
                    ? new GetRecordPageError.NotAllowed()
                    : new GetRecordPageError.NotFound()
            );
        }

        LoadedAnswers? saved = await loader.LoadAnswersAsync(
            loaded,
            forSave: false,
            cancellationToken
        );
        if (saved is null)
        {
            return GetRecordPageResult.Err(new GetRecordPageError.NotFound());
        }

        PageAnswers answers = PageAnswers.For(loaded.Page, saved.Answers);
        var options = await loader.LoadOptionsAsync(loaded.Page, answers, cancellationToken);
        return GetRecordPageResult.Ok(ToDto(loaded, saved, answers, options));
    }

    private static RecordPageDto ToDto(
        LoadedPage loaded,
        LoadedAnswers saved,
        PageAnswers answers,
        Dictionary<string, IReadOnlyList<FormOption>> options
    )
    {
        FormModel model = loaded.Model;
        FormPage page = loaded.Page;
        Applicability applicability = saved.Applicability;
        return new RecordPageDto
        {
            FormVersion = model.Version,
            RecordId = loaded.Record.Id,
            Section = new RecordPageSectionDto { Id = page.SectionId, Title = page.SectionTitle },
            Task = new RecordPageTaskDto { Id = page.TaskId, Title = page.TaskTitle },
            Page = new RecordPagePageDto
            {
                Id = page.Id,
                Title = page.Title,
                CommercialInConfidence = page.CommercialInConfidence,
            },
            PreviousPageId = model.PreviousPage(page, applicability)?.Id,
            NextPageId = model.NextPage(page, applicability)?.Id,
            Questions = page
                .Questions.Select(q =>
                    ToDto(q, options.GetValueOrDefault(q.Id), model, saved.Codes)
                )
                .ToArray(),
            Answers = page.Questions.ToDictionary(
                static q => q.Id,
                q => answers[q.Id].ToJson(),
                StringComparer.Ordinal
            ),
            PageHash = answers.ComputeStableHash(),
        };
    }

    private static RecordPageQuestionDto ToDto(
        FormQuestion question,
        IReadOnlyList<FormOption>? options,
        FormModel model,
        ReferenceCodes codes
    ) =>
        new()
        {
            Id = question.Id,
            Type = question.Control,
            Multiple = question.Shape == AnswerShape.Multi,
            Label = question.Label,
            Hint = question.Hint,
            Display = question.Display,
            Placeholder = question.Placeholder,
            ChildLabel = question.ChildLabel,
            ChildPlaceholder = question.ChildPlaceholder,
            AddAnotherLabel = question.AddAnotherLabel,
            Prefixes = question.Prefixes,
            ShowWhen = question.ShowWhen is null ? null : ToDto(question.ShowWhen, model, codes),
            Rules = question
                .Rules.Select(static r => new RecordPageRuleDto
                {
                    Kind = r.Kind,
                    Value = r.Value,
                    Message = r.Message,
                })
                .ToArray(),
            Options = options?.Select(ToDto).ToArray(),
        };

    /// <summary>
    /// The condition as the browser evaluates it. A reference trigger's codes are sent as the IDs
    /// its options use.
    /// </summary>
    private static RecordPageConditionDto ToDto(
        FormCondition condition,
        FormModel model,
        ReferenceCodes codes
    ) =>
        condition switch
        {
            AnswerInCondition answerIn => new RecordPageConditionDto
            {
                Kind = answerIn.Kind,
                QuestionId = answerIn.QuestionId,
                Values = model.FindQuestion(answerIn.QuestionId)?.ReferenceType is { } type
                    ? [.. codes.IdsOf(type, answerIn.Values)]
                    : answerIn.Values,
            },
            _ => throw new InvalidOperationException(
                $"Unsupported condition kind {condition.Kind}."
            ),
        };

    private static RecordPageOptionDto ToDto(FormOption option) =>
        new()
        {
            Value = option.Value,
            Label = option.Label,
            Hint = option.Hint,
            Children = option.Children.Count == 0 ? null : option.Children.Select(ToDto).ToArray(),
        };
}
