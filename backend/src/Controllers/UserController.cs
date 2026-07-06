using Microsoft.AspNetCore.Mvc;
using UKPS.Api.DTOs;
using UKPS.Api.Services.Interfaces;

namespace UKPS.Api.Controllers;

[ApiController]
[Route("users")]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PaginatedResponseDto<UserListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedResponseDto<UserListItemDto>>> GetUsers(
        [FromQuery] GetUsersQueryDto? getUsersQuery
    )
    {
        if (getUsersQuery is null)
        {
            return BadRequest();
        }

        PaginatedResponseDto<UserListItemDto>? result = await userService.GetUsers(
            getUsersQuery.OrganisationId,
            getUsersQuery.Page,
            getUsersQuery.PageSize,
            getUsersQuery.Status.ToArray()
        );

        return (result is null) ? BadRequest("Organisation not found.") : Ok(result);
    }
}
