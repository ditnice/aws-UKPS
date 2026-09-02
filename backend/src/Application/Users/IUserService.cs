using UKPS.Api.Application.Common;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Application.Users.Errors;

namespace UKPS.Api.Application.Users;

/// <summary>
/// Defines the contract for user-related operations.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Retrieves information about the currently authenticated user.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// the current user's details, including their organisation membership and role.
    /// </returns>
    Task<CurrentUserInformationDto> GetCurrentUser(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a paginated list of users based on the specified criteria.
    /// </summary>
    /// <param name="getUsersQuery">The query parameters used to filter and paginate users.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a
    /// <see cref="Result{TSuccess, TError}"/> object with a paginated response of user list items
    /// or an error of type <see cref="GetUsersError"/>.
    /// </returns>
    Task<Result<PaginatedResponseDto<UserListItemDto>, GetUsersError>> GetUsers(
        GetUsersQueryDto getUsersQuery,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Updates the details of an existing user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to update.</param>
    /// <param name="command">The details to update for the user.</param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a
    /// <see cref="Result{TSuccess, TError}"/> with the updated user's details, or an
    /// error of type <see cref="UpdateUserDetailsError"/> if the user's details could
    /// not be updated.
    /// </returns>
    Task<Result<UserDetailsDto, UpdateUserDetailsError>> UpdateUserDetails(
        int userId,
        UpdateUserDetailsCommand command,
        CancellationToken cancellationToken
    );
}
