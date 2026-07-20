using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UKPS.Api.Common;
using UKPS.Api.DTOs;
using UKPS.Api.Services.Errors;
using UKPS.Api.Services.Interfaces;

namespace UKPS.Api.Controllers;

/// <summary>
/// Controller responsible for managing user-related operations.
/// </summary>
[ApiController]
[Route("users")]
public class UserController(IUserService userService) : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of users based on the specified query parameters.
    /// </summary>
    /// <param name="getUsersQuery">The query parameters for retrieving users, including organisation ID, page, page size, and status filters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A paginated response containing a list of users if successful, or an appropriate error response if the request fails.
    /// </returns>
    /// <response code="200">Returns the paginated list of users.</response>
    /// <response code="400">Returned if the query parameters are invalid or the organisation is not found.</response>
    /// <response code="404">Returned if no users are found matching the query parameters.</response>
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

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="createUserRequestDto">
    /// The details required to create the user.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{TValue}"/> containing the created user's details if the
    /// operation is successful, or an appropriate HTTP error response if the request fails.
    /// </returns>
    [HttpPost]
    [ProducesResponseType<UserDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDetailsDto>> CreateUser(
        [FromBody] CreateUserRequestDto createUserRequestDto
    )
    {
        // Call the UserService.CreateUser(createUserRequestDto)
        Result<UserDetailsDto, CreateUserError> result = await userService.CreateUser(
            createUserRequestDto
        );
        return result.Match<ActionResult<UserDetailsDto>>(
            x => Ok(x), // throw errors on line below
            x =>
                x switch
                {
                    CreateUserError.NotFound => BadRequest(
                        "There is no organisation with that Organisation ID."
                    ),
                    _ => throw new UnreachableException(),
                }
        );
    }
}
