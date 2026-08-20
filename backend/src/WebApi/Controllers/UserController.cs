using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Users;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Application.Users.Errors;

namespace UKPS.Api.WebApi.Controllers;

/// <summary>
/// Controller responsible for managing user-related operations.
/// </summary>
[Authorize]
[ApiController]
[Route("users")]
public class UserController(IUserService userService) : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of users based on the specified query parameters.
    /// </summary>
    /// <param name="getUsersQuery">The query parameters for retrieving users, including organisation ID, page, page size, status, role, and email filters.</param>
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

        var result = await userService.GetUsers(getUsersQuery, cancellationToken);

        return result.Match<ActionResult<PaginatedResponseDto<UserListItemDto>>>(
            items => Ok(items),
            error =>
                error switch
                {
                    GetUsersError.OrganisationNotFound => BadRequest("Organisation not found."),
                    GetUsersError.NotAllowed => Problem(
                        statusCode: StatusCodes.Status403Forbidden,
                        title: "Forbidden",
                        detail: "You are not authorised to view users."
                    ),
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
    /// <param name="cancellationToken">
    /// A token used to cancel the operation>
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{TValue}"/> containing the created user's details when the
    /// operation succeeds. Returns:
    /// <list type="bullet">
    /// <item>
    /// <description><c>400 Bad Request</c> if required data is missing.</description>
    /// </item>
    /// <item>
    /// <description><c>404 Not Found</c> if the specified organisation does not exist.</description>
    /// </item>
    /// <item>
    /// <description><c>409 Conflict</c> if a user with the supplied email address already exists.</description>
    /// </item>
    /// </list>
    /// </returns>
    [HttpPost]
    [ProducesResponseType<UserDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDetailsDto>> CreateUser(
        [FromBody] CreateUserRequestDto createUserRequestDto,
        CancellationToken cancellationToken
    )
    {
        Result<UserDetailsDto, CreateUserError> result = await userService.CreateUser(
            createUserRequestDto,
            cancellationToken
        );
        return result.Match<ActionResult<UserDetailsDto>>(
            x => Ok(x),
            x =>
                x switch
                {
                    CreateUserError.NotFound => NotFound(
                        "There is no organisation with that Organisation ID."
                    ),
                    CreateUserError.MissingFields => BadRequest(
                        "Some of the data required is missing."
                    ),
                    CreateUserError.EmailConflict => Conflict(
                        "A user with that email is already registered."
                    ),
                    _ => throw new UnreachableException(),
                }
        );
    }

    /// <summary>
    /// Updates the details of the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose details are being updated.</param>
    /// <param name="command">The updated user details.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// Returns <see cref="UserDetailsDto"/> with the updated user details when the operation
    /// succeeds (200 OK).
    /// Returns a bad request response (400 Bad Request) when the supplied user details are invalid.
    /// Returns a not found response (404 Not Found) when the specified user does not exist.
    /// Returns a forbidden response (403 Forbidden) when the caller is not authorised to update
    /// the specified user's details.
    /// </returns>
    /// <response code="200">The user's details were successfully updated.</response>
    /// <response code="400">The supplied user details are invalid.</response>
    /// <response code="403">The caller is not authorised to update the specified user's details.</response>
    /// <response code="404">The specified user does not exist.</response>
    [ProducesResponseType<UserDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [HttpPatch("{userId}")]
    public async Task<ActionResult<UserDetailsDto>> UpdateUserDetails(
        int userId,
        [FromBody] UpdateUserDetailsCommand command,
        CancellationToken cancellationToken
    )
    {
        var result = await userService.UpdateUserDetails(userId, command, cancellationToken);

        return result.Match(
            x => Ok(x),
            err =>
            {
                return err.Match<ActionResult<UserDetailsDto>>(
                    unauthorised: () =>
                        Problem(
                            statusCode: StatusCodes.Status403Forbidden,
                            title: "Forbidden",
                            detail: "You are not authorised to update this user's details."
                        ),
                    userDoesNotExist: () =>
                        Problem(
                            statusCode: StatusCodes.Status404NotFound,
                            title: "Not Found",
                            detail: "The specified user does not exist."
                        ),
                    conflictingEmail: () =>
                        Problem(
                            statusCode: StatusCodes.Status409Conflict,
                            title: "Conflict",
                            detail: "The specified email conflicts with an existing email."
                        )
                );
            }
        );
    }
}
