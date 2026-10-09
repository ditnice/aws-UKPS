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
[Authorize]
[ApiController]
[Route("records/{recordId:int}/revisions/{revisionId:int}/pages")]
public class RecordFormController(IRecordPageQueryService recordPageQueryService) : ControllerBase
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

    private ObjectResult PageNotFound() =>
        Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Not Found",
            detail: "The specified page could not be found."
        );
}
