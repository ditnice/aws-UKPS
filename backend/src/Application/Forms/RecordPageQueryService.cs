using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using UKPS.Api.Application.Forms.Bindings;
using UKPS.Api.Application.Forms.Definitions;
using UKPS.Api.Application.Forms.Dtos;
using UKPS.Api.Application.Forms.Errors;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Enums;
using GetRecordPageResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Forms.Dtos.RecordPageDto,
    UKPS.Api.Application.Forms.Errors.GetRecordPageError
>;

namespace UKPS.Api.Application.Forms;

internal sealed class RecordPageQueryService(
    AppDbContext dbContext,
    IOrganisationAuthoriser organisationAuthoriser,
    FormDefinitionRegistry formDefinitions
) : IRecordPageQueryService
{
    public async Task<GetRecordPageResult> GetPage(
        int recordId,
        int revisionId,
        string pageId,
        CancellationToken cancellationToken
    )
    {
        var revision = await dbContext
            .RecordRevisions.AsNoTracking()
            .Where(x => x.Id == revisionId && x.RecordId == recordId)
            .Select(x => new RevisionSummary(
                x.Id,
                x.WorkflowStatus,
                x.Version,
                x.Record!.OrganisationId,
                x.Record.RecordType
            ))
            .SingleOrDefaultAsync(cancellationToken);

        if (revision is null)
        {
            return GetRecordPageResult.Err(new GetRecordPageError.RecordNotFound());
        }

        if (
            !organisationAuthoriser.CanPerformOperationOnOrganisation(
                Operation.Read,
                revision.OrganisationId
            )
        )
        {
            return GetRecordPageResult.Err(new GetRecordPageError.NotAllowed());
        }

        var form = formDefinitions.Get(revision.RecordType);
        var page = form?.FindPage(pageId);
        if (form is null || page is null)
        {
            return GetRecordPageResult.Err(new GetRecordPageError.PageNotFound());
        }

        return GetRecordPageResult.Ok(
            await BuildPageAsync(revision, form, page, cancellationToken)
        );
    }

    private async Task<RecordPageDto> BuildPageAsync(
        RevisionSummary revision,
        FormDefinition form,
        PageDefinition page,
        CancellationToken cancellationToken
    )
    {
        var canEdit =
            revision.WorkflowStatus == WorkflowStatus.Draft
            && organisationAuthoriser.CanPerformOperationOnOrganisation(
                Operation.EditRecordContent,
                revision.OrganisationId
            );

        var answers = await AnswerReader.ReadAsync(
            new BindingContext(dbContext, revision.Id),
            page.Questions,
            cancellationToken
        );

        var questions = new List<FormQuestionDto>(page.Questions.Count);
        foreach (var question in page.Questions)
        {
            questions.Add(await ToDtoAsync(question, answers[question.Id], cancellationToken));
        }

        return new RecordPageDto
        {
            FormVersion = form.Version,
            RevisionVersion = revision.Version,
            ReadOnly = !canEdit,
            OrganisationId = revision.OrganisationId,
            Section = new FormSectionDto { Id = page.SectionId, Title = page.SectionTitle },
            Page = new FormPageDto { Id = page.Id, Title = page.Title },
            PreviousPageId = form.PreviousPage(page)?.Id,
            Questions = questions,
            Answers = answers,
            // No conditions or cross-field rules yet, so no page needs earlier answers.
            Context = new Dictionary<string, JsonNode?>(StringComparer.Ordinal),
        };
    }

    private async Task<FormQuestionDto> ToDtoAsync(
        QuestionDefinition question,
        JsonNode? answer,
        CancellationToken cancellationToken
    )
    {
        var options = question.Options is null
            ? null
            : await question.Options.LoadAsync(
                dbContext,
                AnswerReader.SelectedValues(answer),
                cancellationToken
            );

        return new FormQuestionDto
        {
            Id = question.Id,
            Type = question.Type,
            Label = question.Label,
            Hint = question.Hint,
            Display = question.Display,
            Rules =
            [
                .. question.Rules.Select(rule => new FormRuleDto
                {
                    Kind = rule.Kind,
                    Value = rule.Value,
                    Message = rule.Message,
                }),
            ],
            Options = options
                ?.Select(option => new FormOptionDto { Value = option.Value, Label = option.Label })
                .ToList(),
        };
    }

    private sealed record RevisionSummary(
        int Id,
        WorkflowStatus WorkflowStatus,
        uint Version,
        int OrganisationId,
        RecordType RecordType
    );
}
