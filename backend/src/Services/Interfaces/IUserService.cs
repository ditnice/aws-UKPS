using UKPS.Api.DTOs;
using UKPS.Api.Enums;

namespace UKPS.Api.Services.Interfaces;

public interface IUserService
{
    Task<PaginatedResponseDto<UserListItemDto>?> GetUsers(
        int? organisationId,
        int page,
        int pageSize,
        IReadOnlyCollection<UserOrgStatus> statuses
    );
}
