using Microsoft.EntityFrameworkCore;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Application.Users.Errors;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;
using GetUsersResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Common.PaginatedResponseDto<UKPS.Api.Application.Users.Dtos.UserListItemDto>,
    UKPS.Api.Application.Users.Errors.GetUsersError
>;

namespace UKPS.Api.Application.Users;

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
        IReadOnlyCollection<UserRole> roles,
        string? email,
        CancellationToken cancellationToken
    )
    {
        GetUsersError? organisationError = await ValidateOrganisationAsync(
            organisationId,
            cancellationToken
        );
        if (organisationError is not null)
        {
            return GetUsersResult.Err(organisationError);
        }

        var permittedOrganisationIds = organisationAuthoriser.GetAuthorisedOrganisations(
            Operation.Read
        );
        var organisationMemberships = ApplyFilters(
            dbContext.UserOrgMemberships.AsNoTracking(),
            permittedOrganisationIds,
            organisationId,
            statuses,
            roles,
            email
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

    private async Task<GetUsersError?> ValidateOrganisationAsync(
        int? organisationId,
        CancellationToken cancellationToken
    )
    {
        if (!organisationId.HasValue)
        {
            return null;
        }

        bool actionPermitted = organisationAuthoriser.CanPerformOperationOnOrganisation(
            Operation.Read,
            organisationId.Value
        );
        if (!actionPermitted)
        {
            return new GetUsersError.NotAllowed(organisationId.Value);
        }

        bool organisationExists = await dbContext.Organisations.AnyAsync(
            o => o.Id == organisationId.Value,
            cancellationToken
        );

        return organisationExists
            ? null
            : new GetUsersError.OrganisationNotFound(organisationId.Value);
    }

    private static IQueryable<UserOrgMembership> ApplyFilters(
        IQueryable<UserOrgMembership> input,
        ValueOrAll<int> permittedOrganisationIds,
        int? organisationId,
        IReadOnlyCollection<UserOrgStatus> statuses,
        IReadOnlyCollection<UserRole> roles,
        string? email
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

        if (roles.Count > 0)
        {
            organisationMemberships = organisationMemberships.Where(m =>
                roles.Contains(m.UserRole)
            );
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            string pattern = $"%{EscapeLikePattern(email)}%";
            organisationMemberships = organisationMemberships.Where(m =>
                EF.Functions.ILike(m.User!.WorkEmail, pattern, "\\")
            );
        }

        return organisationMemberships;
    }

    private static string EscapeLikePattern(string value) =>
        value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
}
