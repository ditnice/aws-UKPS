using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UKPS.Api.DTOs;
using UKPS.Api.Services.Errors;
using UKPS.Api.Services.Interfaces;

namespace UKPS.Api.Controllers;

[ApiController]
[Route("users")]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpGet(Name = nameof(GetUsers))]
    [ProducesResponseType<PaginatedResponseDto<UserListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedResponseDto<UserListItemDto>>> GetUsers(
        [FromQuery] GetUsersQueryDto? getUsersQuery,
        CancellationToken cancellationToken
    )
    {
        if (getUsersQuery is null)
        {
            return BadRequest();
        }

        var result = await userService.GetUsers(
            getUsersQuery.OrganisationId,
            getUsersQuery.Page,
            getUsersQuery.PageSize,
            getUsersQuery.Status.ToArray(),
            cancellationToken
        );

        return result.Match<ActionResult<PaginatedResponseDto<UserListItemDto>>>(
            items => Ok(items),
            error =>
                error switch
                {
                    GetUsersError.OrganisationNotFound => BadRequest("Organisation not found."),
                    GetUsersError.NotAllowed => Forbid("User is forbidden from accessing data."),
                    _ => throw new UnreachableException("Unhandled GetUsersError variant."),
                }
        );
    }
}
