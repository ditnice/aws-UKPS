using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Records.Dtos;

namespace UKPS.Api.WebApi.Controllers;

/// <summary>
/// Provides endpoints for retrieving records.
/// </summary>
[Authorize]
[ApiController]
[Route("records")]
public class RecordController : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of records using the supplied filters and sort order.
    /// </summary>
    /// <param name="query">The search, filter, pagination, and sort parameters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A paginated list of record summaries.</returns>
    /// <response code="200">Returns the matching records.</response>
    /// <response code="400">The query parameters are invalid.</response>
    /// <response code="403">The caller is not authorised to view the requested records.</response>
    [HttpGet(Name = nameof(GetRecords))]
    [ProducesResponseType<PaginatedResponseDto<RecordListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<PaginatedResponseDto<RecordListItemDto>> GetRecords(
        [FromQuery] GetRecordsQueryDto query,
        CancellationToken cancellationToken
    )
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }
}
