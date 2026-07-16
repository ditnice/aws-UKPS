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

        var result = await userService.GetUsers(
            getUsersQuery.OrganisationId,
            getUsersQuery.Page,
            getUsersQuery.PageSize,
            getUsersQuery.Status.ToArray()
        );

        return result.Match<ActionResult<PaginatedResponseDto<UserListItemDto>>>(
            items => Ok(items),
            error =>
                error switch
                {
                    GetUsersError.OrganisationNotFound => BadRequest("Organisation not found."),
                    _ => throw new UnreachableException("Unhandled GetUsersError variant."),
                }
        );
    }

    [HttpPost]
    [ProducesResponseType<UserDetailsDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDetailsDto>> CreateUser(
        [FromBody] CreateUserRequestDto createUserRequestDto
    )
    {
        // Call the UserService.CreateUser(createUserRequestDto)
        var result = await userService.CreateUser(createUserRequestDto);
        return result.Match<ActionResult<UserDetailsDto>>(
            x => Ok(x), // throw errors on line below
            x => CreateUserError.OrganisationNotExist => BadRequest("There is no organisation with that Organisation ID. "
        );
    }
}
