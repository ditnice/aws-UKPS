using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UKPS.Api.Application.RecordForms;
using UKPS.Api.Application.RecordForms.Dtos;
using UKPS.Api.Application.RecordForms.Errors;

namespace UKPS.Api.WebApi.Controllers;

/// <summary>Provides the section and task list for a record.</summary>
[Authorize]
[ApiController]
[Route("records/{recordId:int}/task-list")]
public sealed class RecordTaskListController(IRecordTaskListQueryService queryService)
    : ControllerBase
{
    /// <summary>Retrieves the record's sections, task titles, statuses and first-page links.</summary>
    /// <param name="recordId">The record to read.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The task list.</returns>
    /// <response code="200">Returns the task list.</response>
    /// <response code="404">The record is not found or is not accessible to the caller.</response>
    [HttpGet(Name = nameof(GetRecordTaskList))]
    [ProducesResponseType<RecordTaskListDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecordTaskListDto>> GetRecordTaskList(
        int recordId,
        CancellationToken cancellationToken
    )
    {
        GetRecordTaskListResult result = await queryService.GetTaskList(
            recordId,
            cancellationToken
        );

        return result.Match<ActionResult<RecordTaskListDto>>(
            taskList => Ok(taskList),
            error =>
                error switch
                {
                    // ADR-004: not found and not allowed are indistinguishable.
                    GetRecordTaskListError.NotFound or GetRecordTaskListError.NotAllowed =>
                        TaskListNotFound(),
                    _ => throw new UnreachableException(
                        "Unhandled GetRecordTaskListError variant."
                    ),
                }
        );
    }

    private ObjectResult TaskListNotFound() =>
        Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Not Found",
            detail: "The specified task list could not be found."
        );
}
