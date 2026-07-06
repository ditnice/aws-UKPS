using Microsoft.EntityFrameworkCore;
using UKPS.Api.Data;
using UKPS.Api.DTOs;
using UKPS.Api.Enums;
using UKPS.Api.Services.Interfaces;

namespace UKPS.Api.Services;

internal sealed class UserService(AppDbContext dbContext) : IUserService
{
    public async Task<PaginatedResponseDto<UserListItemDto>?> GetUsers(
        int? organisationId,
        int page,
        int pageSize,
        IReadOnlyCollection<UserOrgStatus> statuses
    )
    {
        if (organisationId.HasValue)
        {
            bool organisationExists = await dbContext.Organisations.AnyAsync(o =>
                o.Id == organisationId.Value
            );

            if (!organisationExists)
            {
                return null;
            }
        }

        var organisationMemberships = dbContext.UserOrgMemberships.AsNoTracking();

        if (organisationId.HasValue)
        {
            organisationMemberships = organisationMemberships.Where(m =>
                m.OrganisationId == organisationId.Value
            );
        }

        if (statuses.Count > 0)
        {
            organisationMemberships = organisationMemberships.Where(m =>
                statuses.Contains(m.Status)
            );
        }

        int totalCount = await organisationMemberships.CountAsync();

        List<UserListItemDto> items = await organisationMemberships
            .OrderBy(m => m.User.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new UserListItemDto
            {
                UserId = m.User.Id,
                EmailAddress = m.User.WorkEmail,
                Role = m.UserRole,
                Status = m.Status,
                // No user-level last-active source exists yet.
                LastActive = null,
            })
            .ToListAsync();

        return new PaginatedResponseDto<UserListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }
}
