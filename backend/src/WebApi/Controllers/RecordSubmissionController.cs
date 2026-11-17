using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UKPS.Api.Application.RecordForms;
using UKPS.Api.Application.RecordForms.Dtos;
using UKPS.Api.Application.RecordForms.Errors;
using UKPS.Api.WebApi.CustomResponses;

namespace UKPS.Api.WebApi.Controllers;

/// <summary>
/// Provides the endpoints for checking and submitting a record's draft revision.
/// </summary>
/// <param name="checkAnswersService">Service used to load check your answers.</param>
/// <param name="submissionService">Service used to check and submit records.</param>
[Authorize]
[ApiController]
[Route("records/{recordId:int}")]
public sealed class RecordSubmissionController(
    IRecordCheckAnswersQueryService checkAnswersService,
    IRecordSubmissionService submissionService
) : ControllerBase
{
    /// <summary>
    /// Retrieves every question that applies on the record's latest revision with its answer, the
    /// errors that would stop it being submitted, and the revision hash to submit with.
    /// </summary>
    /// <param name="recordId">The record to check.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The record's answers and errors.</returns>
    /// <response code="200">Returns the record's answers and errors.</response>
    /// <response code="403">The caller can read the record but not submit it.</response>
    /// <response code="404">
    /// The record was not found or is not accessible to the caller.
    /// </response>
    /// <response code="409">
    /// The record can't be submitted in its current state, or the selected organisation differs.
    /// The <c>code</c> member says which.
    /// </response>
    [HttpGet("check-answers", Name = nameof(GetRecordCheckAnswers))]
    [ProducesResponseType<RecordCheckAnswersDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<SubmissionConflictProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RecordCheckAnswersDto>> GetRecordCheckAnswers(
        int recordId,
        CancellationToken cancellationToken
    )
    {
        GetCheckAnswersResult result = await checkAnswersService.GetCheckAnswers(
            recordId,
            cancellationToken
        );

        return result.Match<ActionResult<RecordCheckAnswersDto>>(
            checkAnswers => Ok(checkAnswers),
            error =>
                error.Match(
                    // ADR-004: not found and not allowed are indistinguishable.
                    notFound: _ => RecordNotFound(),
                    notAllowed: _ => RecordNotFound(),
                    cannotSubmit: _ => CannotSubmit(),
                    notSubmittable: _ => NotSubmittable()
                )
        );
    }

    /// <summary>
    /// Submits the record's draft revision, as it was when check your answers loaded. A medicine
    /// goes to QA review.
    /// </summary>
    /// <param name="recordId">The record to submit.</param>
    /// <param name="command">The form version and revision hash check your answers loaded.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="ActionResult"/> indicating whether the record was submitted.</returns>
    /// <response code="204">The record was submitted.</response>
    /// <response code="400">The request body is missing or malformed.</response>
    /// <response code="403">The caller can read the record but not submit it.</response>
    /// <response code="404">
    /// The record was not found or is not accessible to the caller.
    /// </response>
    /// <response code="409">
    /// The record can't be submitted in its current state, or the form or revision changed since
    /// check your answers loaded, or the selected organisation differs. The <c>code</c> member says which.
    /// </response>
    /// <response code="422">
    /// The revision has problems to fix first. <c>errors</c> is keyed by question ID, and
    /// <c>fields</c> lists each problem with its page and task, in form order.
    /// </response>
    [HttpPost("submit", Name = nameof(SubmitRecord))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<SubmissionConflictProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<InvalidRecordProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> SubmitRecord(
        int recordId,
        SubmitRecordCommand command,
        CancellationToken cancellationToken
    )
    {
        SubmitRecordResult result = await submissionService.Submit(
            recordId,
            command,
            cancellationToken
        );

        return result.Match<ActionResult>(NoContent, HandleError);
    }

    private ObjectResult HandleError(SubmitRecordError error)
    {
        return error.Match(
            // ADR-004: not found and not allowed are indistinguishable.
            notFound: _ => RecordNotFound(),
            notAllowed: _ => RecordNotFound(),
            cannotSubmit: _ => CannotSubmit(),
            notSubmittable: _ => NotSubmittable(),
            formVersionChanged: _ =>
                this.ConflictProblem(
                    "form_version_changed",
                    "This form has changed since check your answers was loaded."
                ),
            recordChanged: _ =>
                this.ConflictProblem(
                    "record_changed",
                    "This record has changed since check your answers was loaded."
                ),
            invalid: InvalidRecord
        );
    }

    private ObjectResult RecordNotFound() => this.NotFoundProblem("Record not found.");

    private ObjectResult CannotSubmit() =>
        Problem(
            statusCode: StatusCodes.Status403Forbidden,
            title: "Forbidden",
            detail: "You are not authorised to submit this record."
        );

    private ObjectResult NotSubmittable() =>
        this.ConflictProblem(
            "record_not_submittable",
            "This record can't be submitted because it has already been submitted, or is on hold or archived."
        );

    private ObjectResult InvalidRecord(SubmitRecordError.Invalid invalid)
    {
        foreach (SubmissionErrorDto field in invalid.Errors)
        {
            ModelState.AddModelError(field.QuestionId, field.Message);
        }

        // Shaped as InvalidRecordProblemDetails, which documents it in the OpenAPI schema.
        ValidationProblemDetails problem = ProblemDetailsFactory.CreateValidationProblemDetails(
            HttpContext,
            ModelState,
            statusCode: StatusCodes.Status422UnprocessableEntity,
            detail: "Fix the problems with this record before submitting it."
        );
        problem.Extensions[ControllerProblemExtensions.CodeExtension] =
            InvalidRecordProblemDetails.RecordInvalidCode;
        problem.Extensions[InvalidRecordProblemDetails.FieldsExtension] = invalid.Errors;
        return new ObjectResult(problem) { StatusCode = StatusCodes.Status422UnprocessableEntity };
    }
}
