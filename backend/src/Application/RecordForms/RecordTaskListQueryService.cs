using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using UKPS.Api.Application.RecordForms.Dtos;
using UKPS.Api.Application.RecordForms.Errors;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Persistence;

namespace UKPS.Api.Application.RecordForms;

internal sealed class RecordTaskListQueryService(
    AppDbContext dbContext,
    RecordFormPageLoader loader
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

        HashSet<string> answered = await ReadAnsweredQuestionIdsAsync(
            form.Model,
            form.Revision.Id,
            cancellationToken
        );
        return GetRecordTaskListResult.Ok(
            new RecordTaskListDto
            {
                RecordId = form.Record.Id,
                OrganisationId = form.Record.OrganisationId,
                Sections = form
                    .Model.Sections.Select(section => new RecordTaskListSectionDto
                    {
                        Id = section.Id,
                        Title = section.Title,
                        Tasks = section
                            .Tasks.Select(task => new RecordTaskListTaskDto
                            {
                                Id = task.Id,
                                Title = task.Title,
                                Status = GetStatus(task, answered),
                                FirstPageId = task.FirstPage.Id,
                            })
                            .ToArray(),
                    })
                    .ToArray(),
            }
        );
    }

    private static RecordTaskStatus GetStatus(FormTask task, HashSet<string> answered)
    {
        FormQuestion[] questions = [.. task.Questions];
        bool hasAnyAnswer = questions.Any(question => answered.Contains(question.Id));
        FormQuestion[] required = questions.Where(static q => q.IsRequired).ToArray();
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
