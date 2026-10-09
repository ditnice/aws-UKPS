using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UKPS.Api.Application.Forms;
using UKPS.Api.Application.Forms.Dtos;

namespace UKPS.Api.WebApi.Controllers;

/// <summary>
/// Provides endpoints for viewing and editing a record revision's content, one form page at a
/// time.
/// </summary>
/// <param name="recordPageQueryService">The service used to retrieve form pages.</param>
/// <param name="recordPageSaveService">The service used to save form pages.</param>
[Authorize]
[ApiController]
[Route("records/{recordId:int}/revisions/{revisionId:int}/pages")]
public class RecordFormController(
    IRecordPageQueryService recordPageQueryService,
    IRecordPageSaveService recordPageSaveService
) : ControllerBase
{
    /// <summary>
    /// Retrieves a page of a record revision's content form, with its rules, options and current
    /// answers.
    /// </summary>
    /// <param name="recordId">The record's identifier.</param>
    /// <param name="revisionId">The revision's identifier.</param>
    /// <param name="pageId">The page's identifier.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The form page.</returns>
    /// <response code="200">
    /// Returns the page. <c>readOnly</c> is true if the caller cannot edit it.
    /// </response>
    /// <response code="404">
    /// The record, revision or page does not exist or is not accessible to the caller.
    /// </response>
    [HttpGet("{pageId}", Name = nameof(GetRecordPage))]
    [ProducesResponseType<RecordPageDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecordPageDto>> GetRecordPage(
        [FromRoute] int recordId,
        [FromRoute] int revisionId,
        [FromRoute] string pageId,
        CancellationToken cancellationToken
    )
    {
        var result = await recordPageQueryService.GetPage(
            recordId,
            revisionId,
            pageId,
            cancellationToken
        );

        // Records the caller cannot read share a response with missing ones so that record
        // identifiers cannot be enumerated (ADR-004).
        return result.Match<ActionResult<RecordPageDto>>(
            page => Ok(page),
            error =>
                error.Match(
                    recordNotFound: _ => PageNotFound(),
                    notAllowed: _ => PageNotFound(),
                    pageNotFound: _ => PageNotFound()
                )
        );
    }

    /// <summary>
    /// Validates and saves every answer on a page of a draft revision, and returns the page to
    /// show next.
    /// </summary>
    /// <param name="recordId">The record's identifier.</param>
    /// <param name="revisionId">The revision's identifier.</param>
    /// <param name="pageId">The page's identifier.</param>
    /// <param name="command">The page's answers and the versions it was loaded with.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The next page.</returns>
    /// <response code="200">The page was saved (or nothing changed).</response>
    /// <response code="400">
    /// One or more answers are invalid. Errors are keyed by question ID.
    /// </response>
    /// <response code="403">The caller can view the record but not edit its content.</response>
    /// <response code="404">
    /// The record, revision or page does not exist or is not accessible to the caller.
    /// </response>
    /// <response code="409">
    /// The page cannot be saved as loaded. <c>code</c> is <c>revision_not_draft</c>,
    /// <c>revision_changed</c> or <c>form_version_changed</c>; reload the page.
    /// </response>
    [HttpPut("{pageId}", Name = nameof(SaveRecordPage))]
    [ProducesResponseType<SaveRecordPageDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SaveRecordPageDto>> SaveRecordPage(
        [FromRoute] int recordId,
        [FromRoute] int revisionId,
        [FromRoute] string pageId,
        [FromBody] SaveRecordPageCommand command,
        CancellationToken cancellationToken
    )
    {
        var result = await recordPageSaveService.SavePage(
            recordId,
            revisionId,
            pageId,
            command,
            cancellationToken
        );

        return result.Match<ActionResult<SaveRecordPageDto>>(
            saved => Ok(saved),
            error =>
                error.Match<ActionResult<SaveRecordPageDto>>(
                    recordNotFound: _ => PageNotFound(),
                    notAllowed: _ => PageNotFound(),
                    pageNotFound: _ => PageNotFound(),
                    editNotAllowed: _ =>
                        Problem(
                            statusCode: StatusCodes.Status403Forbidden,
                            title: "Forbidden",
                            detail: "You are not authorised to edit this record."
                        ),
                    revisionNotDraft: _ =>
                        PageConflict(
                            "revision_not_draft",
                            "This record can no longer be edited because it has been submitted."
                        ),
                    revisionChanged: _ =>
                        PageConflict(
                            "revision_changed",
                            "This record has changed since the page was loaded."
                        ),
                    formVersionChanged: _ =>
                        PageConflict(
                            "form_version_changed",
                            "This form has changed since the page was loaded."
                        ),
                    invalid: invalid =>
                    {
                        foreach (var (key, messages) in invalid.Errors)
                        {
                            foreach (var message in messages)
                            {
                                ModelState.AddModelError(key, message);
                            }
                        }

                        return ValidationProblem(ModelState);
                    }
                )
        );
    }

    private ObjectResult PageConflict(string code, string detail)
    {
        var problem = ProblemDetailsFactory.CreateProblemDetails(
            HttpContext,
            statusCode: StatusCodes.Status409Conflict,
            title: "Conflict",
            detail: detail
        );
        problem.Extensions["code"] = code;
        return new ObjectResult(problem) { StatusCode = StatusCodes.Status409Conflict };
    }

    private ObjectResult PageNotFound() =>
        Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Not Found",
            detail: "The specified page could not be found."
        );
}
