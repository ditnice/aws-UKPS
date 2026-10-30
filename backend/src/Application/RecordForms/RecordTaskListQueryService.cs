using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.RecordForms.Dtos;
using UKPS.Api.Application.RecordForms.Errors;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.RecordWorkflow;

namespace UKPS.Api.Application.RecordForms;

internal sealed class RecordTaskListQueryService(
    AppDbContext dbContext,
    IOrganisationAuthoriser organisationAuthoriser,
    FormModelRegistry formModels
) : IRecordTaskListQueryService
{
    private static readonly MethodInfo _readFactsMethod =
        typeof(RecordTaskListQueryService).GetMethod(
            nameof(ReadFactsAsync),
            BindingFlags.Static | BindingFlags.NonPublic
        )!;

    public async Task<GetRecordTaskListResult> GetTaskList(
        int recordId,
        CancellationToken cancellationToken
    )
    {
        RecordRevision? revision = await dbContext
            .RecordRevisions.AsNoTracking()
            .Include(static r => r.Record)
            .Where(r => r.RecordId == recordId)
            .OrderByDescending(static r => r.CreatedAt)
            .ThenByDescending(static r => r.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (revision?.Record is not { } record)
        {
            return GetRecordTaskListResult.Err(new GetRecordTaskListError.NotFound());
        }

        if (
            !organisationAuthoriser.CanPerformOperationOnOrganisation(
                Operation.Read,
                record.OrganisationId
            )
        )
        {
            return GetRecordTaskListResult.Err(new GetRecordTaskListError.NotAllowed());
        }

        FormModel? model = formModels.Find(record.RecordType);
        if (model is null)
        {
            return GetRecordTaskListResult.Err(new GetRecordTaskListError.NotFound());
        }

        List<RecordTaskListSectionDto> sections = await BuildSectionsAsync(
            dbContext,
            model,
            revision.Id,
            cancellationToken
        );
        return GetRecordTaskListResult.Ok(
            new RecordTaskListDto
            {
                RecordId = record.Id,
                OrganisationId = record.OrganisationId,
                Sections = sections,
            }
        );
    }

    private static async Task<List<RecordTaskListSectionDto>> BuildSectionsAsync(
        AppDbContext dbContext,
        FormModel model,
        int revisionId,
        CancellationToken cancellationToken
    )
    {
        List<RecordTaskListSectionDto> sections = [];
        foreach (FormSection section in model.Sections)
        {
            List<RecordTaskListTaskDto> tasks = [];
            foreach (FormTask task in section.Tasks)
            {
                RecordTaskStatus status = await GetStatusAsync(
                    dbContext,
                    task,
                    revisionId,
                    cancellationToken
                );
                tasks.Add(
                    new RecordTaskListTaskDto
                    {
                        Id = task.Id,
                        Title = task.Title,
                        Status = status,
                        FirstPageId = task.FirstPage.Id,
                    }
                );
            }

            sections.Add(
                new RecordTaskListSectionDto
                {
                    Id = section.Id,
                    Title = section.Title,
                    Tasks = tasks,
                }
            );
        }

        return sections;
    }

    private static async Task<RecordTaskStatus> GetStatusAsync(
        AppDbContext dbContext,
        FormTask task,
        int revisionId,
        CancellationToken cancellationToken
    )
    {
        bool hasAnyAnswer = false;
        bool requiredAnswersComplete = true;
        bool hasRequiredQuestions = task.Questions.Any(static question => question.IsRequired);
        foreach (
            IGrouping<FormEntity, FormQuestion> group in task.Questions.GroupBy(static question =>
                question.Entity
            )
        )
        {
            FormQuestion[] questions = group.ToArray();
            bool entityHasRequiredQuestions = questions.Any(static question => question.IsRequired);
            TaskAnswerFacts? facts = await ReadFactsForEntityAsync(
                dbContext,
                group.Key,
                questions,
                revisionId,
                cancellationToken
            );

            hasAnyAnswer |= facts?.HasAnyAnswer == true;
            if (entityHasRequiredQuestions && facts?.RequiredAnswersComplete != true)
            {
                requiredAnswersComplete = false;
            }
        }

        if (requiredAnswersComplete && (hasRequiredQuestions || hasAnyAnswer))
        {
            return RecordTaskStatus.Completed;
        }

        return hasAnyAnswer ? RecordTaskStatus.InProgress : RecordTaskStatus.NotStarted;
    }

    private static Task<TaskAnswerFacts?> ReadFactsForEntityAsync(
        AppDbContext dbContext,
        FormEntity entity,
        IReadOnlyList<FormQuestion> questions,
        int revisionId,
        CancellationToken cancellationToken
    ) =>
        (Task<TaskAnswerFacts?>)
            _readFactsMethod
                .MakeGenericMethod(entity.ClrType)
                .Invoke(null, [questions, revisionId, dbContext, cancellationToken])!;

    private static Task<TaskAnswerFacts?> ReadFactsAsync<TEntity>(
        IReadOnlyList<FormQuestion> questions,
        int revisionId,
        AppDbContext dbContext,
        CancellationToken cancellationToken
    )
        where TEntity : class
    {
        ParameterExpression row = Expression.Parameter(typeof(TEntity), "row");
        Expression[] answers = questions
            .Select(question => question.Binding.HasAnswerExpression(row))
            .ToArray();
        Expression hasAnyAnswer = answers.Aggregate(Expression.OrElse);

        Expression[] requiredAnswers = questions
            .Where(static question => question.IsRequired)
            .Select(question => question.Binding.HasAnswerExpression(row))
            .ToArray();
        Expression requiredAnswersComplete =
            requiredAnswers.Length == 0
                ? Expression.Constant(true)
                : requiredAnswers.Aggregate(Expression.AndAlso);

        ConstructorInfo constructor = typeof(TaskAnswerFacts).GetConstructors().Single();
        Expression body = Expression.New(constructor, hasAnyAnswer, requiredAnswersComplete);
        Expression<Func<TEntity, TaskAnswerFacts>> selector = Expression.Lambda<
            Func<TEntity, TaskAnswerFacts>
        >(body, row);

        return dbContext
            .Set<TEntity>()
            .AsNoTracking()
            .Where(entityRow => EF.Property<int>(entityRow, "RevisionId") == revisionId)
            .Select(selector)
            .SingleOrDefaultAsync(cancellationToken);
    }

    private sealed record TaskAnswerFacts(bool HasAnyAnswer, bool RequiredAnswersComplete);
}
