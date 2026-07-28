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
}
