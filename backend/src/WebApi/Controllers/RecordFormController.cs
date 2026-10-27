using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UKPS.Api.Application.RecordForms;
using UKPS.Api.Application.RecordForms.Dtos;
using UKPS.Api.Application.RecordForms.Errors;

namespace UKPS.Api.WebApi.Controllers;

/// <summary>
/// Provides endpoints for reading and saving record content form pages.
/// </summary>
/// <param name="queryService">Service used to load form pages.</param>
/// <param name="saveService">Service used to validate and save form pages.</param>
[Authorize]
[ApiController]
[Route("records/{recordId:int}/pages/{pageId}")]
public class RecordFormController(
    IRecordFormQueryService queryService,
    IRecordFormSaveService saveService
) : ControllerBase
{
    private const string CodeExtension = "code";

    /// <summary>
    /// Retrieves a page of a record's content form: its questions, rules, options and the current
    /// answers on the record's latest revision.
    /// </summary>
    /// <param name="recordId">The record to read.</param>
    /// <param name="pageId">The page to read.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The page.</returns>
    /// <response code="200">Returns the page.</response>
    /// <response code="404">
    /// The record or page was not found or is not accessible to the caller.
    /// </response>
    [HttpGet(Name = nameof(GetRecordFormPage))]
    [ProducesResponseType<RecordPageDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecordPageDto>> GetRecordFormPage(
        int recordId,
        string pageId,
        CancellationToken cancellationToken
    )
    {
        GetRecordPageResult result = await queryService.GetPage(
            recordId,
            pageId,
            cancellationToken
        );

        return result.Match<ActionResult<RecordPageDto>>(
            page => Ok(page),
            error =>
                error switch
                {
                    // ADR-004: not found and not allowed are indistinguishable.
                    GetRecordPageError.NotFound or GetRecordPageError.NotAllowed => PageNotFound(),
                    _ => throw new UnreachableException("Unhandled GetRecordPageError variant."),
                }
        );
    }

    /// <summary>
    /// Validates and saves every answer on a page of a record's content form, to the record's
    /// latest revision, which must be a draft.
    /// </summary>
    /// <param name="recordId">The record to write.</param>
    /// <param name="pageId">The page to save.</param>
    /// <param name="command">Every answer on the page, with the versions it was loaded at.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The page to go to next.</returns>
    /// <response code="200">Saved, or nothing changed. Returns the next page.</response>
    /// <response code="400">One or more answers are missing, malformed or invalid.</response>
    /// <response code="403">The caller can read the record but not edit it.</response>
    /// <response code="404">
    /// The record or page was not found or is not accessible to the caller.
    /// </response>
    /// <response code="409">
    /// The revision is not a draft, or the form or revision changed since the page was loaded.
    /// The <c>code</c> member says which.
    /// </response>
    [HttpPut(Name = nameof(SaveRecordFormPage))]
    [ProducesResponseType<SaveRecordPageDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SaveRecordPageDto>> SaveRecordFormPage(
        int recordId,
        string pageId,
        SaveRecordPageCommand command,
        CancellationToken cancellationToken
    )
    {
        SaveRecordPageResult result = await saveService.SavePage(
            recordId,
            pageId,
            command,
            cancellationToken
        );

        return result.Match<ActionResult<SaveRecordPageDto>>(
            saved => Ok(saved),
            error =>
                error switch
                {
                    SaveRecordPageError.NotFound or SaveRecordPageError.NotAllowed =>
                        PageNotFound(),
                    SaveRecordPageError.CannotEdit => Problem(
                        statusCode: StatusCodes.Status403Forbidden,
                        title: "Forbidden",
                        detail: "You are not authorised to edit this record."
                    ),
                    SaveRecordPageError.NotDraft => Conflict(
                        "revision_not_draft",
                        "This record can no longer be edited because it has been submitted."
                    ),
                    SaveRecordPageError.RevisionChanged => Conflict(
                        "revision_changed",
                        "This record has changed since the page was loaded."
                    ),
                    SaveRecordPageError.FormVersionChanged => Conflict(
                        "form_version_changed",
                        "This form has changed since the page was loaded."
                    ),
                    SaveRecordPageError.Invalid invalid => InvalidAnswers(invalid),
                    _ => throw new UnreachableException("Unhandled SaveRecordPageError variant."),
                }
        );
    }

    private ObjectResult PageNotFound() =>
        Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Not Found",
            detail: "The specified page could not be found."
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

    private ActionResult InvalidAnswers(SaveRecordPageError.Invalid invalid)
    {
        foreach ((string questionId, string message) in invalid.Errors)
        {
            ModelState.AddModelError(questionId, message);
        }

        return ValidationProblem(ModelState);
    }
}
