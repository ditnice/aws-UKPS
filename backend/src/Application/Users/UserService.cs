using Microsoft.EntityFrameworkCore;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Application.InternalServices.Temporal;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Application.Users.Errors;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;
using GetUsersResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Common.PaginatedResponseDto<UKPS.Api.Application.Users.Dtos.UserListItemDto>,
    UKPS.Api.Application.Users.Errors.GetUsersError
>;
using UpdateUserDetailsResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Users.Dtos.UserDetailsDto,
    UKPS.Api.Application.Users.Errors.UpdateUserDetailsError
>;

namespace UKPS.Api.Application.Users;

internal sealed class UserService(
    AppDbContext dbContext,
    IOrganisationAuthoriser organisationAuthoriser,
    IDateTimeProvider timeProvider,
    IIdentityService identityService,
    ICurrentUserInfoService currentUserInfoService
) : IUserService
{
    public async Task<GetUsersResult> GetUsers(
        GetUsersQueryDto getUsersQuery,
        CancellationToken cancellationToken
    )
    {
        GetUsersError? organisationError = await ValidateOrganisationAsync(
            getUsersQuery.OrganisationId,
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
            getUsersQuery
        );

        int totalCount = await organisationMemberships.CountAsync(cancellationToken);

        List<UserListItemDto> items = await organisationMemberships
            .OrderBy(m => m.User!.Id)
            .Skip((getUsersQuery.Page - 1) * getUsersQuery.PageSize)
            .Take(getUsersQuery.PageSize)
            .Select(m => new UserListItemDto
            {
                UserId = m.User!.Id,
                EmailAddress = m.User.WorkEmail,
                Role = m.UserRole,
                Status = m.Status,
                LastActive = m.User.LastActive,
            })
            .ToListAsync(cancellationToken);

        return GetUsersResult.Ok(
            new PaginatedResponseDto<UserListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = getUsersQuery.Page,
                PageSize = getUsersQuery.PageSize,
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
        GetUsersQueryDto getUsersQuery
    )
    {
        IQueryable<UserOrgMembership> organisationMemberships = input
            .Where(permittedOrganisationIds.Contains<UserOrgMembership>(x => x.OrganisationId))
            .Where(m => m.Status != UserOrgStatus.Rejected);

        if (getUsersQuery.OrganisationId.HasValue)
        {
            organisationMemberships = organisationMemberships.Where(m =>
                m.OrganisationId == getUsersQuery.OrganisationId.Value
            );
        }

        if (getUsersQuery.Status.Count > 0)
        {
            organisationMemberships = organisationMemberships.Where(m =>
                getUsersQuery.Status.Contains(m.Status)
            );
        }

        if (getUsersQuery.Role.Count > 0)
        {
            organisationMemberships = organisationMemberships.Where(m =>
                getUsersQuery.Role.Contains(m.UserRole)
            );
        }

        if (!string.IsNullOrWhiteSpace(getUsersQuery.Email))
        {
            string pattern = $"%{EscapeLikePattern(getUsersQuery.Email)}%";
            organisationMemberships = organisationMemberships.Where(m =>
                EF.Functions.ILike(m.User!.WorkEmail, pattern, "\\")
            );
        }

        if (getUsersQuery.LastActiveFrom.HasValue)
        {
            DateTime from = getUsersQuery.LastActiveFrom.Value.UtcDateTime;
            organisationMemberships = organisationMemberships.Where(m =>
                m.User!.LastActive != null && m.User.LastActive >= from
            );
        }

        if (getUsersQuery.LastActiveTo.HasValue)
        {
            DateTime to = getUsersQuery.LastActiveTo.Value.UtcDateTime;
            organisationMemberships = organisationMemberships.Where(m =>
                m.User!.LastActive != null && m.User.LastActive <= to
            );
        }

        return organisationMemberships;
    }

    public async Task<UpdateUserDetailsResult> UpdateUserDetails(
        int userId,
        UpdateUserDetailsCommand command,
        CancellationToken cancellationToken
    )
    {
        User? user = await dbContext.Users.FindAsync([userId], cancellationToken);

        if (user is null)
        {
            return UpdateUserDetailsResult.Err(new UpdateUserDetailsError.UserDoesNotExist());
        }

        CurrentUser currentUser = currentUserInfoService.GetCurrentUserInfo();
        bool isTheCurrentUserModifyingTheirOwnDetails = string.Equals(
            currentUser.Email,
            user.WorkEmail,
            StringComparison.Ordinal
        );

        if (!isTheCurrentUserModifyingTheirOwnDetails)
        {
            return UpdateUserDetailsResult.Err(new UpdateUserDetailsError.Unauthorised());
        }

        user.UpdateDetails(
            command.FullName,
            command.WorkTelephone,
            command.WorkEmail,
            timeProvider.GetUtcNow()
        );

        if (user.Events.OfType<User.EmailUpdatedEvent>().Any())
        {
            await identityService.UpdateUserEmail(
                user.CognitoUsername,
                command.WorkEmail,
                cancellationToken
            );
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        UserDetailsDto output = MapToDto(user);
        return UpdateUserDetailsResult.Ok(output);
    }

    private static UserDetailsDto MapToDto(User user)
    {
        return new()
        {
            UserType = user.UserType,
            Title = user.Title,
            FullName = user.FullName,
            JobTitle = user.JobTitle,
            WorkPhone = user.WorkTelephone,
            WorkEmail = user.WorkEmail,
        };
    }

    private static string EscapeLikePattern(string value) =>
        value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
}
