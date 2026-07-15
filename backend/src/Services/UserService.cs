using Microsoft.EntityFrameworkCore;
using UKPS.Api.Data;
using UKPS.Api.DTOs;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Enums;
using UKPS.Api.Services.Errors;
using UKPS.Api.Services.Interfaces;
using GetUsersResult = UKPS.Api.Common.Result<
    UKPS.Api.DTOs.PaginatedResponseDto<UKPS.Api.DTOs.UserListItemDto>,
    UKPS.Api.Services.Errors.GetUsersError
>;

namespace UKPS.Api.Services;

internal sealed class UserService(
    AppDbContext dbContext,
    IOrganisationAuthoriser organisationAuthoriser
) : IUserService
{
    public async Task<GetUsersResult> GetUsers(
        int? organisationId,
        int page,
        int pageSize,
        IReadOnlyCollection<UserOrgStatus> statuses,
        CancellationToken cancellationToken
    )
    {
        if (organisationId.HasValue)
        {
            bool actionPermitted = organisationAuthoriser.CanPerformOperationOnOrganisation(
                Operation.Read,
                organisationId.Value
            );
            if (!actionPermitted)
            {
                return GetUsersResult.Err(new GetUsersError.NotAllowed(organisationId.Value));
            }
            bool organisationExists = await dbContext.Organisations.AnyAsync(
                o => o.Id == organisationId.Value,
                cancellationToken
            );

            if (!organisationExists)
            {
                return GetUsersResult.Err(
                    new GetUsersError.OrganisationNotFound(organisationId.Value)
                );
            }
        }

        var permittedOrganisationIds = organisationAuthoriser.GetAuthorisedOrganisations(
            Operation.Read
        );
        var organisationMemberships = ApplyFilters(
            dbContext.UserOrgMemberships.AsNoTracking(),
            permittedOrganisationIds,
            organisationId,
            statuses
        );

        int totalCount = await organisationMemberships.CountAsync(cancellationToken);

        List<UserListItemDto> items = await organisationMemberships
            .OrderBy(m => m.User!.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new UserListItemDto
            {
                UserId = m.User!.Id,
                EmailAddress = m.User.WorkEmail,
                Role = m.UserRole,
                Status = m.Status,
                // No user-level last-active source exists yet.
                LastActive = null,
            })
            .ToListAsync(cancellationToken);

        return GetUsersResult.Ok(
            new PaginatedResponseDto<UserListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
            }
        );
    }

    private static IQueryable<UserOrgMembership> ApplyFilters(
        IQueryable<UserOrgMembership> input,
        ValueOrAll<int> permittedOrganisationIds,
        int? organisationId,
        IReadOnlyCollection<UserOrgStatus> statuses
    )
    {
        IQueryable<UserOrgMembership> organisationMemberships = input.Where(
            permittedOrganisationIds.Contains<UserOrgMembership>(x => x.OrganisationId)
        );

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

        return organisationMemberships;
    }
}
