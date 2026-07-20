using UKPS.Api.Common;
using UKPS.Api.DTOs;
using UKPS.Api.Enums;
using UKPS.Api.Services.Errors;

namespace UKPS.Api.Services.Interfaces;

public interface IUserService
{
    Task<Result<UserDetailsDto, CreateUserError>> CreateUser(
        CreateUserRequestDto createUserRequestDto
    );
    Task<Result<PaginatedResponseDto<UserListItemDto>, GetUsersError>> GetUsers(
        int? organisationId,
        int page,
        int pageSize,
        IReadOnlyCollection<UserOrgStatus> statuses
    );
}
