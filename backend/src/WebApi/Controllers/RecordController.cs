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
public class RecordController : ControllerBase
{
    private readonly IRecordCreationService _recordCreationService;
    private readonly IRecordService _recordService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordController"/> class.
    /// </summary>
    /// <param name="recordCreationService">The service used to create records.</param>
    /// <param name="recordService">The service used to retrieve records.</param>
    public RecordController(
        IRecordCreationService recordCreationService,
        IRecordService recordService
    )
    {
        _recordCreationService = recordCreationService;
        _recordService = recordService;
    }

    /// <summary>
    /// Retrieves a paginated list of records belonging to the user's organisation.
    /// </summary>
    /// <param name="organisationId">The unique identifier of the organisation.</param>
    /// <param name="getRecordQuery">The search, filter, pagination, and sort parameters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A paginated list of record summaries.</returns>
    /// <response code="200">Returns the matching records.</response>
    /// <response code="400">The query parameters are invalid.</response>
    [HttpGet("organisations/{organisationId:int}", Name = nameof(GetOrganisationRecords))]
    [ProducesResponseType<PaginatedResponseDto<RecordListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResponseDto<RecordListItemDto>>> GetOrganisationRecords(
        [FromRoute] int organisationId,
        [FromQuery] GetRecordsQueryDto? getRecordQuery,
        CancellationToken cancellationToken
    )
    {
        if (getRecordQuery is null)
        {
            return BadRequest();
        }

        var result = await _recordService.GetOrganisationRecords(
            organisationId,
            getRecordQuery,
            cancellationToken
        );

        return result.Match<ActionResult<PaginatedResponseDto<RecordListItemDto>>>(
            items => Ok(items),
            error =>
                error switch
                {
                    GetRecordsError.OrganisationNotFound => Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Bad Request",
                        detail: "Organisation not found."
                    ),
                    GetRecordsError.NotAllowed => Problem(
                        statusCode: StatusCodes.Status403Forbidden,
                        title: "Forbidden",
                        detail: "You are not authorised to view records."
                    ),
                    _ => throw new UnreachableException("Unhandled GetRecordsError variant."),
                }
        );
    }

    /// <summary>
    /// Creates a new record.
    /// </summary>
    /// <returns>The created record.</returns>
    /// <response code="200">Returns the created record.</response>
    /// <response code="400">The request body is invalid.</response>
    /// <response code="403">The caller is not authorised to create the requested record.</response>
    [HttpPost(Name = nameof(CreateRecord))]
    [ProducesResponseType<CreateRecordDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CreateRecordDto>> CreateRecord(
        CreateRecordCommand command,
        CancellationToken cancellationToken
    )
    {
        CreateRecordResult result = await _recordCreationService.CreateRecord(
            command,
            cancellationToken
        );
        return result.Match(
            x => Ok(x),
            err =>
                err.Match<ActionResult<CreateRecordDto>>(
                    notAuthorised: _ =>
                        Problem(
                            statusCode: StatusCodes.Status403Forbidden,
                            title: "Forbidden",
                            detail: "You are not authorised to create a record for this organisation."
                        ),
                    organisationDoesNotExist: _ =>
                        Problem(
                            statusCode: StatusCodes.Status400BadRequest,
                            title: "Organisation not found",
                            detail: "The specified organisation does not exist or is invalid."
                        )
                )
        );
    }
}
