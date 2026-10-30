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

        FormPage page = loaded.Page;
        var rows = await loader.LoadRowsAsync(
            page,
            loaded.Revision.Id,
            track: false,
            cancellationToken
        );
        Dictionary<string, FormAnswer> answers = RecordFormPageLoader.ReadAnswers(page, rows);
        var options = await loader.LoadOptionsAsync(page, answers, cancellationToken);

        return GetRecordPageResult.Ok(
            new RecordPageDto
            {
                FormVersion = loaded.Model.Version,
                RevisionVersion = loaded.Revision.Version,
                RecordId = recordId,
                Section = new RecordPageSectionDto
                {
                    Id = page.SectionId,
                    Title = page.SectionTitle,
                },
                Page = new RecordPagePageDto { Id = page.Id, Title = page.Title },
                PreviousPageId = loaded.Model.PreviousPage(page)?.Id,
                NextPageId = loaded.Model.NextPage(page)?.Id,
                Questions = page
                    .Questions.Select(q => ToDto(q, options.GetValueOrDefault(q.Id)))
                    .ToArray(),
                Answers = answers.ToDictionary(
                    static a => a.Key,
                    static a => a.Value.ToJson(),
                    StringComparer.Ordinal
                ),
            }
        );
    }

    private static RecordPageQuestionDto ToDto(
        FormQuestion question,
        IReadOnlyList<FormOption>? options
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

    private static RecordPageOptionDto ToDto(FormOption option) =>
        new()
        {
            Value = option.Value,
            Label = option.Label,
            Children = option.Children.Count == 0 ? null : option.Children.Select(ToDto).ToArray(),
        };
}
