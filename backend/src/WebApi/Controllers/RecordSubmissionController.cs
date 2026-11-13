using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UKPS.Api.Application.RecordForms;
using UKPS.Api.Application.RecordForms.Dtos;
using UKPS.Api.Application.RecordForms.Errors;
using UKPS.Api.WebApi.CustomResponses;

namespace UKPS.Api.WebApi.Controllers;

/// <summary>
/// Provides the endpoint for submitting a record's draft revision.
/// </summary>
/// <param name="submissionService">Service used to check and submit records.</param>
[Authorize]
[ApiController]
[Route("records/{recordId:int}")]
public class RecordSubmissionController(IRecordSubmissionService submissionService) : ControllerBase
{
    private const string CodeExtension = "code";

    /// <summary>
    /// Submits the record's draft revision, as it was when check your answers loaded. A medicine
    /// goes to QA review.
    /// </summary>
    /// <param name="recordId">The record to submit.</param>
    /// <param name="command">The form version and revision hash check your answers loaded.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="ActionResult"/> indicating whether the record was submitted.</returns>
    /// <response code="200">The record was submitted.</response>
    /// <response code="400">The request body is missing or malformed.</response>
    /// <response code="403">The caller can read the record but not submit it.</response>
    /// <response code="404">
    /// The record was not found or is not accessible to the caller.
    /// </response>
    /// <response code="409">
    /// The record can't be submitted in its current state, or the form or revision changed since
    /// check your answers loaded. The <c>code</c> member says which.
    /// </response>
    /// <response code="422">
    /// The revision has problems to fix first. <c>errors</c> is keyed by question ID, and
    /// <c>fields</c> lists each problem with its page and task, in form order.
    /// </response>
    [HttpPost("submit", Name = nameof(SubmitRecord))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
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

        return result.Match<ActionResult>(Ok, HandleError);
    }

    private ObjectResult HandleError(SubmitRecordError error)
    {
        return error.Match(
            // ADR-004: not found and not allowed are indistinguishable.
            notFound: _ => RecordNotFound(),
            notAllowed: _ => RecordNotFound(),
            cannotSubmit: _ =>
                Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Forbidden",
                    detail: "You are not authorised to submit this record."
                ),
            notSubmittable: _ =>
                Conflict(
                    "record_not_submittable",
                    "This record can't be submitted because it has already been submitted, or is on hold or archived."
                ),
            formVersionChanged: _ =>
                Conflict(
                    "form_version_changed",
                    "This form has changed since check your answers was loaded."
                ),
            recordChanged: _ =>
                Conflict(
                    "record_changed",
                    "This record has changed since check your answers was loaded."
                ),
            invalid: InvalidRecord
        );
    }

    private ObjectResult RecordNotFound() =>
        Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Not Found",
            detail: "Record not found."
        );

    private ObjectResult Conflict(string code, string detail)
    {
        ProblemDetails problem = ProblemDetailsFactory.CreateProblemDetails(
            HttpContext,
            statusCode: StatusCodes.Status409Conflict,
            title: "Conflict",
            detail: detail
        );
        problem.Extensions[CodeExtension] = code;
        return new ObjectResult(problem) { StatusCode = StatusCodes.Status409Conflict };
    }

    private ObjectResult InvalidRecord(SubmitRecordError.Invalid invalid)
    {
        foreach (SubmissionErrorDto field in invalid.Errors)
        {
            ModelState.AddModelError(field.QuestionId, field.Message);
        }

        InvalidRecordProblemDetails problem = new(
            ProblemDetailsFactory.CreateValidationProblemDetails(
                HttpContext,
                ModelState,
                statusCode: StatusCodes.Status422UnprocessableEntity,
                detail: "Fix the problems with this record before submitting it."
            ),
            invalid.Errors
        );
        return new ObjectResult(problem) { StatusCode = StatusCodes.Status422UnprocessableEntity };
    }
}
