using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Records;
using UKPS.Api.Application.Records.Dtos;
using UKPS.Api.Application.Records.Errors;

namespace UKPS.Api.WebApi.Controllers;

/// <summary>
/// Provides endpoints for retrieving records.
/// </summary>
[Authorize]
[ApiController]
[Route("records")]
public class RecordController(IRecordService recordService) : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of records using the supplied filters and sort order.
    /// </summary>
    /// <param name="getRecordQuery">The search, filter, pagination, and sort parameters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A paginated list of record summaries.</returns>
    /// <response code="200">Returns the matching records.</response>
    /// <response code="400">The query parameters are invalid.</response>
    /// <response code="403">The caller is not authorised to view the requested records.</response>
    [HttpGet(Name = nameof(GetRecords))]
    [ProducesResponseType<PaginatedResponseDto<RecordListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedResponseDto<RecordListItemDto>>> GetRecords(
        [FromQuery] GetRecordsQueryDto? getRecordQuery,
        CancellationToken cancellationToken
    )
    {
        if (getRecordQuery is null)
        {
            return BadRequest();
        }

        var result = await recordService.GetRecords(getRecordQuery, cancellationToken);

        return result.Match<ActionResult<PaginatedResponseDto<RecordListItemDto>>>(
            items => Ok(items),
            error =>
                error switch
                {
                    GetRecordsError.OrganisationNotFound => BadRequest("Organisation not found."),
                    GetRecordsError.NotAllowed => Problem(
                        statusCode: StatusCodes.Status403Forbidden,
                        title: "Forbidden",
                        detail: "You are not authorised to view records."
                    ),
                    _ => throw new UnreachableException("Unhandled GetRecordsError variant."),
                }
        );
    }
}
