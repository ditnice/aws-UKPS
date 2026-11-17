using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.RecordForms.Dtos;
using UKPS.Api.Application.RecordForms.Errors;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Options;

namespace UKPS.Api.Application.RecordForms;

internal sealed class RecordCheckAnswersQueryService(
    RecordFormPageLoader loader,
    RevisionChecker revisionChecker,
    IOrganisationAuthoriser organisationAuthoriser
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

        LoadedRevision revision = await revisionChecker.LoadAsync(form, cancellationToken);
        RevisionCheck check = await revisionChecker.CheckAsync(form, revision, cancellationToken);
        Summary summary = new(form.Model, revision, check);
        return GetCheckAnswersResult.Ok(
            new RecordCheckAnswersDto
            {
                FormVersion = form.Model.Version,
                RecordId = form.Record.Id,
                OrganisationId = form.Record.OrganisationId,
                RevisionHash = revision.Hash,
                CanSubmit =
                    form.Record.CanSubmit(form.Revision)
                    && organisationAuthoriser.CanPerformOperationOnOrganisation(
                        Operation.EditRecordContent,
                        form.Record.OrganisationId
                    ),
                Sections =
                [
                    .. form
                        .Model.Sections.Select(summary.ToDto)
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
    private sealed record Summary(FormModel Model, LoadedRevision Revision, RevisionCheck Check)
    {
        public RecordCheckAnswersSectionDto ToDto(FormSection section) =>
            new()
            {
                Id = section.Id,
                Title = section.Title,
                Tasks =
                [
                    .. section.Tasks.Select(ToDto).Where(static task => task.Questions.Count > 0),
                ],
            };

        private RecordCheckAnswersTaskDto ToDto(FormTask task) =>
            new()
            {
                Id = task.Id,
                Title = task.Title,
                Questions = [.. task.Questions.Where(Revision.Applicability.Applies).Select(ToDto)],
            };

        private RecordCheckAnswersQuestionDto ToDto(FormQuestion question)
        {
            FormAnswer answer = Revision.Answers[question.Id];
            return new()
            {
                Id = question.Id,
                PageId = Model.PageOf(question).Id,
                Label = question.Label,
                Type = question.Control,
                Multiple = question.Shape == AnswerShape.Multi,
                Values = [.. answer.AllValues()],
                SelectedOptions = Check.Options.TryGetValue(
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
