using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Records;
using UKPS.Api.Application.Records.Dtos;
using UKPS.Api.Application.Records.Dtos.RecordDetails;
using UKPS.Api.Application.Records.Errors;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.WebApi.Controllers;

/// <summary>
/// Provides endpoints for retrieving and creating records.
/// </summary>
/// <param name="recordService">Service used to search and filter records.</param>
/// <param name="recordViewService">Service used to retrieve record data for viewing.</param>
/// <param name="recordCreationService">Service used to create records.</param>
[Authorize]
[ApiController]
[Route("records")]
public class RecordController(
    IRecordService recordService,
    IRecordViewService recordViewService,
    IRecordCreationService recordCreationService
) : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of records belonging to the user's organisation.
    /// </summary>
    /// <param name="organisationId">The unique identifier of the organisation.</param>
    /// <param name="getRecordQuery">The search, filter, pagination, and sort parameters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A paginated list of record summaries.</returns>
    /// <response code="200">Returns the matching records.</response>
    /// <response code="400">The query parameters are invalid.</response>
    /// <response code="403">The caller is not authorised to view the organisation's records.</response>
    /// <response code="404">The specified organisation does not exist.</response>
    [HttpGet("organisations/{organisationId:int}", Name = nameof(GetOrganisationRecords))]
    [ProducesResponseType<PaginatedResponseDto<RecordListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        var result = await recordService.GetOrganisationRecords(
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
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Not Found",
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
    /// Retrieves the data held on the latest revision of a record, whatever its status.
    /// </summary>
    /// <param name="id">The identifier of the record to retrieve.</param>
    /// <param name="recordType">The expected type of the record.</param>
    /// <param name="organisationId">The organisation the record is expected to belong to.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The record data.</returns>
    /// <response code="200">Returns the record data.</response>
    /// <response code="400">The record type or organisation is missing or invalid.</response>
    /// <response code="404">
    /// The requested record was not found in the organisation or is not accessible to the caller.
    /// </response>
    [HttpGet("{id:int}", Name = nameof(GetRecord))]
    [ProducesResponseType<RecordDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecordDto>> GetRecord(
        int id,
        [FromQuery, Required] RecordType? recordType,
        [FromQuery, Required] int? organisationId,
        CancellationToken cancellationToken
    )
    {
        var result = await recordViewService.GetRecord(
            id,
            recordType!.Value,
            organisationId!.Value,
            cancellationToken
        );

        // Returned directly rather than via Ok() so it serialises with its recordType.
        return result.Match<ActionResult<RecordDto>>(
            record => record,
            error =>
                error switch
                {
                    GetRecordError.NotAllowed
                    or GetRecordError.NotFound
                    or GetRecordError.RecordTypeMismatch => NotFound(),
                    _ => throw new UnreachableException("Unhandled GetRecordError variant."),
                }
        );
    }

    /// <summary>
    /// Creates a new record.
    /// </summary>
    /// <param name="command">The details of the record to create.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The created record.</returns>
    /// <response code="200">Returns the created record.</response>
    /// <response code="400">
    /// The request body is invalid, or the specified organisation does not exist.
    /// </response>
    /// <response code="403">The caller is not authorised to create the requested record.</response>
    [HttpPost(Name = nameof(CreateRecord))]
    [ProducesResponseType<CreateRecordDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CreateRecordDto>> CreateRecord(
        CreateRecordCommand command,
        CancellationToken cancellationToken
    )
    {
        CreateRecordResult result = await recordCreationService.CreateRecord(
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
                    {
                        ModelState.AddModelError(
                            nameof(CreateRecordCommand.OrganisationId),
                            "The specified organisation does not exist or is invalid."
                        );
                        return ValidationProblem(ModelState);
                    }
                )
        );
    }

    /// <summary>
    /// Discards a record that has never been submitted.
    /// </summary>
    /// <param name="recordId">The identifier of the record to discard.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="ActionResult"/> indicating whether the record was discarded.</returns>
    /// <response code="200">The record was discarded.</response>
    /// <response code="400">The record has been submitted, so cannot be discarded.</response>
    /// <response code="404">
    /// The record was not found, has already been discarded, or is not accessible to the caller.
    /// </response>
    /// <response code="409">The record was updated by another operation while it was being discarded.</response>
    [HttpPatch("{recordId:int}/discard", Name = nameof(DiscardRecord))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> DiscardRecord(int recordId, CancellationToken cancellationToken)
    {
        Result<DiscardRecordError> result = await recordService.DiscardDraftRecord(
            recordId,
            cancellationToken
        );

        if (result.IsOk)
        {
            return Ok();
        }

        // Callers who cannot access the record get the same response as a missing record, so
        // record identifiers from other organisations are not revealed.
        return result.Error switch
        {
            DiscardRecordError.RecordNotFound or DiscardRecordError.NotAllowed => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found",
                detail: "Record not found."
            ),
            DiscardRecordError.InvalidDiscard => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Record cannot be discarded",
                detail: "Only records that have never been submitted can be discarded."
            ),
            DiscardRecordError.ConcurrencyError => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Record update conflict",
                detail: "The record could not be discarded because it was updated by another operation. Please try again."
            ),
            _ => throw new UnreachableException("Unhandled DiscardRecordError variant."),
        };
    }
}
