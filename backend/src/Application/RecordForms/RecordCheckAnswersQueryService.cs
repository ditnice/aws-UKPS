using System.Diagnostics;
using UKPS.Api.Application.RecordForms.Dtos;
using UKPS.Api.Application.RecordForms.Errors;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Options;

namespace UKPS.Api.Application.RecordForms;

internal sealed class RecordCheckAnswersQueryService(
    RecordFormPageLoader loader,
    RevisionChecker revisionChecker
) : IRecordCheckAnswersQueryService
{
    public async Task<GetCheckAnswersResult> GetCheckAnswers(
        int recordId,
        CancellationToken cancellationToken
    )
    {
        var (form, failure) = await loader.LoadFormAsync(recordId, track: false, cancellationToken);
        if (form is null)
        {
            return GetCheckAnswersResult.Err(
                failure == PageLookupFailure.NotAllowed
                    ? new GetCheckAnswersError.NotAllowed()
                    : new GetCheckAnswersError.NotFound()
            );
        }

        // Check your answers is only for submitting, so it is refused as submit would be.
        GetCheckAnswersError? blocked = revisionChecker.FindSubmitBlocker(form) switch
        {
            null => null,
            SubmitBlocker.CannotSubmit => new GetCheckAnswersError.CannotSubmit(),
            SubmitBlocker.NotSubmittable => new GetCheckAnswersError.NotSubmittable(),
            { } blocker => throw new UnreachableException($"Unhandled submit blocker {blocker}."),
        };
        if (blocked is not null)
        {
            return GetCheckAnswersResult.Err(blocked);
        }

        LoadedRevision revision = await revisionChecker.LoadAsync(form, cancellationToken);
        RevisionCheck check = await revisionChecker.CheckAsync(form, revision, cancellationToken);
        SummaryBuilder summary = new(form.Model, revision, check);
        return GetCheckAnswersResult.Ok(
            new RecordCheckAnswersDto
            {
                FormVersion = form.Model.Version,
                RecordId = form.Record.Id,
                OrganisationId = form.Record.OrganisationId,
                RevisionHash = revision.Hash,
                Sections =
                [
                    .. form
                        .Model.Sections.Select(summary.ToSection)
                        .Where(static section => section.Tasks.Count > 0),
                ],
                Errors = check.Errors,
            }
        );
    }

    /// <summary>
    /// Builds the summary rows of the questions that apply, leaving out tasks and sections with
    /// none.
    /// </summary>
    private sealed class SummaryBuilder(
        FormModel model,
        LoadedRevision revision,
        RevisionCheck check
    )
    {
        public RecordCheckAnswersSectionDto ToSection(FormSection section) =>
            new()
            {
                Id = section.Id,
                Title = section.Title,
                Tasks =
                [
                    .. section.Tasks.Select(ToTask).Where(static task => task.Questions.Count > 0),
                ],
            };

        private RecordCheckAnswersTaskDto ToTask(FormTask task) =>
            new()
            {
                Id = task.Id,
                Title = task.Title,
                Questions =
                [
                    .. task.Questions.Where(revision.Applicability.Applies).Select(ToQuestion),
                ],
            };

        private RecordCheckAnswersQuestionDto ToQuestion(FormQuestion question)
        {
            FormAnswer answer = revision.Answers[question.Id];
            return new()
            {
                Id = question.Id,
                PageId = model.PageOf(question).Id,
                Label = question.Label,
                Type = question.Control,
                Multiple = question.Shape == AnswerShape.Multi,
                Values = [.. answer.AllValues()],
                SelectedOptions = check.Options.TryGetValue(
                    question.Id,
                    out IReadOnlyList<FormOption>? options
                )
                    ? SelectedOptions(answer, options)
                    : null,
            };
        }

        /// <summary>
        /// The options the answer chose, in the order offered. A value that is not offered is left
        /// out; it is reported as an error.
        /// </summary>
        /// <remarks>
        /// Options are at most two levels deep, which the tree option provider enforces, so a
        /// chosen option is either a parent or one of its children.
        /// </remarks>
        private static RecordCheckAnswersOptionDto[] SelectedOptions(
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
                        .Select(option => new RecordCheckAnswersOptionDto
                        {
                            Value = option.Value,
                            Label = option.Label,
                            ParentLabel = ReferenceEquals(option, parent) ? null : parent.Label,
                        })
                ),
            ];
        }
    }
}
