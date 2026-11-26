using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using UKPS.Api.Application.RecordForms.Conditions;
using UKPS.Api.Application.RecordForms.Dtos;
using UKPS.Api.Application.RecordForms.Errors;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.Records;
using UKPS.Api.Application.Records.Dtos;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.RecordForms;

internal sealed class RecordTaskListQueryService(
    AppDbContext dbContext,
    RecordFormPageLoader loader,
    IRecordViewService recordViewService
) : IRecordTaskListQueryService
{
    private static readonly MethodInfo _readAnsweredMethod =
        typeof(RecordTaskListQueryService).GetMethod(
            nameof(ReadAnsweredAsync),
            BindingFlags.Static | BindingFlags.NonPublic
        )!;

    public async Task<GetRecordTaskListResult> GetTaskList(
        int recordId,
        CancellationToken cancellationToken
    )
    {
        var (form, failure) = await loader.LoadFormAsync(recordId, track: false, cancellationToken);
        if (form is null)
        {
            return GetRecordTaskListResult.Err(
                failure == PageLookupFailure.NotAllowed
                    ? new GetRecordTaskListError.NotAllowed()
                    : new GetRecordTaskListError.NotFound()
            );
        }

        if (
            !(
                await recordViewService.GetRecordOverview(
                    form.Record.Id,
                    form.Record.RecordType,
                    form.Record.OrganisationId,
                    cancellationToken
                )
            ).TryGetSuccessValue(out RecordOverviewDto? overview)
        )
        {
            return GetRecordTaskListResult.Err(new GetRecordTaskListError.NotFound());
        }

        HashSet<string> answered = await ReadAnsweredQuestionIdsAsync(
            form.Model,
            form.Revision.Id,
            cancellationToken
        );
        Applicability applicability = await loader.LoadApplicabilityAsync(
            form.Model,
            form.Revision.Id,
            cancellationToken
        );
        return GetRecordTaskListResult.Ok(
            new RecordTaskListDto
            {
                RecordId = form.Record.Id,
                OrganisationId = form.Record.OrganisationId,
                DisplayStatus = overview.DisplayStatus,
                CompanyCode = overview.CompanyCode,
                RecordTitle = overview.RecordTitle,
                RecordType = overview.RecordType,
                CanDiscard = overview.Actions.Contains(RecordAction.Discard),
                Sections = form
                    .Model.Sections.Select(section => ToDto(section, answered, applicability))
                    .ToArray(),
            }
        );
    }

    private static RecordTaskListSectionDto ToDto(
        FormSection section,
        HashSet<string> answered,
        Applicability applicability
    ) =>
        new()
        {
            Id = section.Id,
            Title = section.Title,
            Optional = section.Optional,
            SubText = section.SubText,
            Tasks = section
                .Tasks.Select(task => new RecordTaskListTaskDto
                {
                    Id = task.Id,
                    Title = task.Title,
                    Hint = task.Hint,
                    Status = GetStatus(task, answered, applicability),
                    FirstPageId = task.FirstPage.Id,
                })
                .ToArray(),
        };

    /// <summary>
    /// The task's status, counting only the questions that apply: a hidden question is neither an
    /// answer nor a required question still to answer.
    /// </summary>
    private static RecordTaskStatus GetStatus(
        FormTask task,
        HashSet<string> answered,
        Applicability applicability
    )
    {
        // A task whose first page depends on an unanswered question cannot start yet (PIM and EAMS
        // before the technology status is answered). Once it is answered, the task is either not
        // applicable or startable.
        if (task.FirstPage.ShowWhen?.TriggerQuestionIds.Any(id => !answered.Contains(id)) == true)
        {
            return RecordTaskStatus.CannotStart;
        }

        FormQuestion[] applying = task.Questions.Where(applicability.Applies).ToArray();
        if (applying.Length == 0)
        {
            return RecordTaskStatus.NotApplicable;
        }

        bool hasAnyAnswer = applying.Any(question => answered.Contains(question.Id));
        FormQuestion[] required = applying.Where(static q => q.IsRequired).ToArray();
        bool requiredAnswersComplete = required.All(question => answered.Contains(question.Id));

        if (requiredAnswersComplete && (required.Length > 0 || hasAnyAnswer))
        {
            return RecordTaskStatus.Completed;
        }

        return hasAnyAnswer ? RecordTaskStatus.InProgress : RecordTaskStatus.NotStarted;
    }

    /// <summary>
    /// Reads which of the form's questions have a saved answer on the revision, with one query
    /// per content entity.
    /// </summary>
    private async Task<HashSet<string>> ReadAnsweredQuestionIdsAsync(
        FormModel model,
        int revisionId,
        CancellationToken cancellationToken
    )
    {
        HashSet<string> answered = new(StringComparer.Ordinal);
        foreach (
            IGrouping<FormEntity, FormQuestion> group in model
                .Pages.SelectMany(static page => page.Questions)
                .GroupBy(static question => question.Entity)
        )
        {
            FormQuestion[] questions = [.. group];
            bool[]? flags = await (Task<bool[]?>)
                _readAnsweredMethod
                    .MakeGenericMethod(group.Key.ClrType)
                    .Invoke(null, [dbContext, questions, revisionId, cancellationToken])!;
            for (int i = 0; i < questions.Length; i++)
            {
                if (flags?[i] == true)
                {
                    answered.Add(questions[i].Id);
                }
            }
        }

        return answered;
    }

    /// <summary>
    /// Projects the revision's row to one flag per question, in the order given, computed in the
    /// database. Returns null when the revision has no row.
    /// </summary>
    private static Task<bool[]?> ReadAnsweredAsync<TEntity>(
        AppDbContext dbContext,
        IReadOnlyList<FormQuestion> questions,
        int revisionId,
        CancellationToken cancellationToken
    )
        where TEntity : class
    {
        ParameterExpression row = Expression.Parameter(typeof(TEntity), "row");
        Expression<Func<TEntity, bool[]?>> selector = Expression.Lambda<Func<TEntity, bool[]?>>(
            Expression.NewArrayInit(
                typeof(bool),
                questions.Select(question => question.Binding.HasAnswerExpression(row))
            ),
            row
        );

        return dbContext
            .Set<TEntity>()
            .AsNoTracking()
            .Where(entityRow => EF.Property<int>(entityRow, "RevisionId") == revisionId)
            .Select(selector)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
