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
    /// Retrieves a user's details along with their role within the specified organisation.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to retrieve.</param>
    /// <param name="organisationId">
    /// The unique identifier of the organisation to read the user's membership from. A user may
    /// belong to several organisations, so their role is resolved against this organisation.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="UserInformationDto"/> describing the user and their organisation membership if
    /// successful, or an appropriate error response if the request fails.
    /// </returns>
    /// <response code="200">Returns the user's details and their role within the organisation.</response>
    /// <response code="400">Returned if the specified organisation does not exist.</response>
    /// <response code="403">Returned if the caller is not authorised to view the organisation's users.</response>
    /// <response code="404">Returned if the user is not a member of the specified organisation.</response>
    [HttpGet(
        "{userId:int}/organisations/{organisationId:int}",
        Name = nameof(GetUserDetailsWithinOrganisation)
    )]
    [ProducesResponseType<UserInformationDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserInformationDto>> GetUserDetailsWithinOrganisation(
        int userId,
        int organisationId,
        CancellationToken cancellationToken
    )
    {
        var result = await userService.GetUserDetailsWithinOrganisation(
            userId,
            organisationId,
            cancellationToken
        );

        return result.Match<ActionResult<UserInformationDto>>(
            user => Ok(user),
            error =>
                error switch
                {
                    GetUsersError.OrganisationNotFound => BadRequest("Organisation not found."),
                    GetUsersError.UserNotFound => NotFound(
                        "The user is not a member of this organisation."
                    ),
                    GetUsersError.NotAllowed => Problem(
                        statusCode: StatusCodes.Status403Forbidden,
                        title: "Forbidden",
                        detail: "You are not authorised to view this user."
                    ),
                    _ => throw new UnreachableException("Unhandled GetUsersError variant."),
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
    /// <response code="409">The request conflicts with the existing data such as another users email.</response>
    [ProducesResponseType<UserDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPatch("{userId}")]
    public async Task<ActionResult<UserDetailsDto>> UpdateUserDetails(
        [FromRoute] int userId,
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
