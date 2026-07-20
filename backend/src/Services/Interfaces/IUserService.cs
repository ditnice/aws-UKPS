using UKPS.Api.Common;
using UKPS.Api.DTOs;
using UKPS.Api.Enums;
using UKPS.Api.Services.Errors;

namespace UKPS.Api.Services.Interfaces;

/// <summary>
/// Defines the contract for user-related operations.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="createUserRequestDto">
    /// The details required to create the user.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a
    /// <see cref="Result{TSuccess, TError}"/> object with the created user's details
    /// or an error of type <see cref="CreateUserError"/>.
    /// </returns>
    Task<Result<UserDetailsDto, CreateUserError>> CreateUser(
        CreateUserRequestDto createUserRequestDto
    );

    /// <summary>
    /// Retrieves a paginated list of users based on the specified criteria.
    /// </summary>
    /// <param name="organisationId">The optional ID of the organisation to filter users by.</param>
    /// <param name="page">The page number to retrieve.</param>
    /// <param name="pageSize">The number of users per page.</param>
    /// <param name="statuses">A collection of user organisation statuses to filter by.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a
    /// <see cref="Result{TSuccess, TError}"/> object with a paginated response of user list items
    /// or an error of type <see cref="GetUsersError"/>.
    /// </returns>
    Task<Result<PaginatedResponseDto<UserListItemDto>, GetUsersError>> GetUsers(
        int? organisationId,
        int page,
        int pageSize,
        IReadOnlyCollection<UserOrgStatus> statuses,
        CancellationToken cancellationToken
    );
}
