using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Application.InternalServices.Temporal;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Application.Users.Errors;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Configurations;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;
using GetUserInformationResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Users.Dtos.UserInformationDto,
    UKPS.Api.Application.Users.Errors.GetUsersError
>;
using GetUsersResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Common.PaginatedResponseDto<UKPS.Api.Application.Users.Dtos.UserListItemDto>,
    UKPS.Api.Application.Users.Errors.GetUsersError
>;
using UpdateUserDetailsResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Users.Dtos.UserDetailsDto,
    UKPS.Api.Application.Users.Errors.UpdateUserDetailsError
>;

namespace UKPS.Api.Application.Users;

internal partial class UserService(
    AppDbContext dbContext,
    IOrganisationAuthoriser organisationAuthoriser,
    IDateTimeProvider timeProvider,
    IIdentityService identityService,
    ICurrentUserInfoService currentUserInfoService,
    ILogger<UserService> logger
) : IUserService
{
    public async Task<UserInformationDto> GetCurrentUser(CancellationToken cancellationToken)
    {
        CurrentUser currentUser = currentUserInfoService.GetCurrentUserInfo();
        User? possibleUser = await dbContext
            .Users.Include(x => x.UserOrgMemberships)!
                .ThenInclude(x => x.Organisation)
            .FirstOrDefaultAsync(
                x => x.CognitoUsername == currentUser.CognitoUsername,
                cancellationToken
            );
        User user =
            possibleUser
            ?? throw new InvalidOperationException(
                "Could not find current user by email in the database as expected."
            );
        var membership =
            user.UserOrgMemberships!.FirstOrDefault(x =>
                x.OrganisationId == currentUser.OrganisationId
            )
            ?? throw new InvalidOperationException(
                "Current user did not have the membership as expected."
            );

        return new UserInformationDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            WorkTelephone = user.WorkTelephone ?? string.Empty,
            WorkEmail = currentUser.Email,
            OrganisationMembershipId = membership.Id,
            OrganisationId = membership.Organisation!.Id,
            OrganisationName = membership.Organisation.OrganisationName,
            UserRole = currentUser.UserRole,
        };
    }

    public async Task<GetUsersResult> GetUsers(
        GetUsersQueryDto getUsersQuery,
        CancellationToken cancellationToken
    )
    {
        GetUsersError? organisationError = await ValidateOrganisationAsync(
            getUsersQuery.OrganisationId,
            Operation.Read,
            cancellationToken
        );
        if (organisationError is not null)
        {
            return GetUsersResult.Err(organisationError);
        }

        var permittedOrganisationIds = organisationAuthoriser.GetAuthorisedOrganisations(
            Operation.Read
        );
        IQueryable<UserInformationTrackingProjection> unionQuery = GetProjectedUserInformation();

        IQueryable<UserInformationTrackingProjection> organisationMemberships = ApplyFilters(
            unionQuery,
            permittedOrganisationIds,
            getUsersQuery
        );

        int totalCount = await organisationMemberships.CountAsync(cancellationToken);

        IQueryable<UserInformationTrackingProjection> orderedOrganisationMemberships = Sort(
            organisationMemberships,
            getUsersQuery.SortBy,
            getUsersQuery.SortDirection
        );
        var items = await orderedOrganisationMemberships
            .AsNoTracking()
            .Skip((getUsersQuery.Page - 1) * getUsersQuery.PageSize)
            .Take(getUsersQuery.PageSize)
            .ToListAsync(cancellationToken);

        var projectedItems = items
            .Select(m => new UserListItemDto
            {
                UserId = m.UserId,
                RegistrationRequestId = m.RegistrationRequestId,
                EmailAddress = m.WorkEmail,
                Role = m.UserRole,
                Status = m.Status,
                LastActive = m.LastActive,
                Actions = GetPermittedActions(m),
            })
            .ToArray();

        return GetUsersResult.Ok(
            new PaginatedResponseDto<UserListItemDto>
            {
                Items = projectedItems,
                TotalCount = totalCount,
                Page = getUsersQuery.Page,
                PageSize = getUsersQuery.PageSize,
            }
        );
    }

    private IQueryable<UserInformationTrackingProjection> GetProjectedUserInformation()
    {
        var userOrgMembershipProjection = dbContext.UserOrgMemberships.Select(x => new
        {
            UserId = (int?)x.User!.Id,
            RegistrationRequestId = (int?)null,
            x.User.WorkEmail,
            x.UserRole,
            Status = (UserOrgStatus)x.Status,
            MembershipStatus = (UserOrgMembershipStatus?)x.Status,
            OrganisationId = x.Organisation!.Id,
            x.User.LastActive,
        });
        var filteredValues = dbContext.UserRegistrationRequests.Where(x => x.RejectedAt == null);
        var mostRecentValues = filteredValues.Where(x =>
            x.CreatedAt
            == filteredValues.Where(y => y.WorkEmail == x.WorkEmail).Max(y => y.CreatedAt)
        );
        var userRegistrationRequestsProjections = mostRecentValues.Select(x => new
        {
            UserId = (int?)null,
            RegistrationRequestId = (int?)x.Id,
            x.WorkEmail,
            UserRole = UserRole.Standard,
            Status = x.RejectedAt == null ? UserOrgStatus.RequestedAccess : UserOrgStatus.Rejected,
            MembershipStatus = (UserOrgMembershipStatus?)null,
            OrganisationId = x.Organisation!.Id,
            LastActive = (DateTime?)null,
        });

        // The Join below is necessary as EF core cannot handle CognitoUsername
        // being on one side of the union but not on the other. Future work to look
        // if there's a better approach.
        return userOrgMembershipProjection
            .Union(userRegistrationRequestsProjections)
            .GroupJoin(
                dbContext.Users,
                x => x.UserId,
                x => x.Id,
                (x, matches) => new { x, matches }
            )
            .SelectMany(
                x => x.matches.DefaultIfEmpty(),
                (a, b) =>
                    new UserInformationTrackingProjection()
                    {
                        UserId = a.x.UserId,
                        RegistrationRequestId = a.x.RegistrationRequestId,
                        WorkEmail = a.x.WorkEmail,
                        UserRole = a.x.UserRole,
                        Status = a.x.Status,
                        MembershipStatus = a.x.MembershipStatus,
                        OrganisationId = a.x.OrganisationId,
                        LastActive = a.x.LastActive,
                        CognitoUsername = b == null ? null : b.CognitoUsername,
                    }
            );
    }

    private UserMembershipAction[] GetPermittedActions(UserInformationTrackingProjection m)
    {
        var currentUserInfo = currentUserInfoService.GetCurrentUserInfo();

        if (m.CognitoUsername is not null && currentUserInfo.CognitoUsername == m.CognitoUsername)
        {
            return [];
        }

        if (currentUserInfo.UserRole is not (UserRole.Champion or UserRole.Super))
        {
            return [];
        }

        if (m.RegistrationRequestId.HasValue)
        {
            return [UserMembershipAction.ApproveMembership, UserMembershipAction.RejectMembership];
        }

        if (m.MembershipStatus.HasValue)
        {
            return UserOrgMembership.GetPermittedActions(m.MembershipStatus.Value).ToArray();
        }

        throw new InvalidOperationException(
            "UserInformationTrackingProjection was in an invalid state"
        );
    }

    public async Task<GetUserInformationResult> GetUserDetailsWithinOrganisation(
        int userId,
        int organisationId,
        CancellationToken cancellationToken
    )
    {
        GetUsersError? organisationError = await ValidateOrganisationAsync(
            organisationId,
            Operation.ElevatedRead,
            cancellationToken
        );
        if (organisationError is not null)
        {
            return GetUserInformationResult.Err(organisationError);
        }

        UserInformationDto? user = await dbContext
            .UserOrgMemberships.AsNoTracking()
            .Where(m => m.UserId == userId && m.OrganisationId == organisationId)
            .Select(m => new UserInformationDto
            {
                UserId = m.User!.Id,
                FullName = m.User.FullName,
                WorkTelephone = m.User.WorkTelephone ?? string.Empty,
                WorkEmail = m.User.WorkEmail,
                OrganisationMembershipId = m.Id,
                OrganisationId = m.OrganisationId,
                OrganisationName = m.Organisation!.OrganisationName,
                UserRole = m.UserRole,
            })
            .FirstOrDefaultAsync(cancellationToken);

        return user is null
            ? GetUserInformationResult.Err(new GetUsersError.UserNotFound(userId, organisationId))
            : GetUserInformationResult.Ok(user);
    }

    private static IQueryable<UserInformationTrackingProjection> Sort(
        IQueryable<UserInformationTrackingProjection> value,
        GetUsersQuerySortValue sortBy,
        SortDirection sortDirection
    )
    {
        Expression<Func<UserInformationTrackingProjection, object?>> sortExpression = sortBy switch
        {
            GetUsersQuerySortValue.LastActive => m =>
                m.LastActive == null ? DateTime.MinValue : m.LastActive.Value,
            GetUsersQuerySortValue.Email => m => m.WorkEmail,
            GetUsersQuerySortValue.Role => m => m.UserRole,
            GetUsersQuerySortValue.Status => m => m.Status,
            _ => throw new ArgumentOutOfRangeException(
                nameof(sortBy),
                $"Unexpected value: {sortBy}"
            ),
        };
        return sortDirection switch
        {
            SortDirection.Ascending => value
                .OrderBy(sortExpression)
                .ThenBy(x => x.UserId != null ? x.UserId : x.RegistrationRequestId),
            SortDirection.Descending => value
                .OrderByDescending(sortExpression)
                .ThenByDescending(x => x.UserId != null ? x.UserId : x.RegistrationRequestId),
            _ => throw new ArgumentOutOfRangeException(
                nameof(sortDirection),
                $"Unexpected value: {sortDirection}"
            ),
        };
    }

    private async Task<GetUsersError?> ValidateOrganisationAsync(
        int? organisationId,
        Operation operation,
        CancellationToken cancellationToken
    )
    {
        if (!organisationId.HasValue)
        {
            return null;
        }

        bool actionPermitted = organisationAuthoriser.CanPerformOperationOnOrganisation(
            operation,
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

    private static IQueryable<UserInformationTrackingProjection> ApplyFilters(
        IQueryable<UserInformationTrackingProjection> input,
        ValueOrAll<int> permittedOrganisationIds,
        GetUsersQueryDto getUsersQuery
    )
    {
        IQueryable<UserInformationTrackingProjection> organisationMemberships = input
            .Where(
                permittedOrganisationIds.Contains<UserInformationTrackingProjection>(x =>
                    x.OrganisationId
                )
            )
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
                EF.Functions.ILike(m.WorkEmail, pattern, "\\")
            );
        }

        if (getUsersQuery.LastActiveFrom.HasValue)
        {
            DateTime from = getUsersQuery.LastActiveFrom.Value.UtcDateTime;
            organisationMemberships = organisationMemberships.Where(m =>
                m.LastActive != null && m.LastActive >= from
            );
        }

        if (getUsersQuery.LastActiveTo.HasValue)
        {
            DateTime to = getUsersQuery.LastActiveTo.Value.UtcDateTime;
            organisationMemberships = organisationMemberships.Where(m =>
                m.LastActive != null && m.LastActive <= to
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
        await using IDbContextTransaction transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            User? user = await dbContext.Users.FindAsync([userId], cancellationToken);

            if (user is null)
            {
                return UpdateUserDetailsResult.Err(new UpdateUserDetailsError.UserDoesNotExist());
            }

            bool isTheCurrentUserModifyingTheirOwnDetails = currentUserInfoService.IsCurrentUser(
                user.WorkEmail
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

            await dbContext.SaveChangesAsync(cancellationToken);
            await HandleUserEvents(user, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            UserDetailsDto output = MapToDto(user);
            return UpdateUserDetailsResult.Ok(output);
        }
        catch (DbUpdateException updateException)
            when (updateException.InnerException is PostgresException postgresException
                && string.Equals(
                    postgresException.ConstraintName,
                    ConstraintNames.UserUniqueEmail,
                    StringComparison.Ordinal
                )
            )
        {
            await transaction.RollbackAsync(cancellationToken);
            return UpdateUserDetailsResult.Err(new UpdateUserDetailsError.ConflictingEmail());
        }
        catch (Exception ex)
        {
            LogUpdatingUserDetailsFailed(ex);
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task HandleUserEvents(User user, CancellationToken cancellationToken)
    {
        foreach (var ev in user.Events)
        {
            switch (ev)
            {
                case User.EmailUpdatedEvent emailUpdatedEvent:
                    await HandleUserEvent(user, emailUpdatedEvent, cancellationToken);
                    break;
            }
        }
    }

    private async Task HandleUserEvent(
        User user,
        User.EmailUpdatedEvent emailUpdatedEvent,
        CancellationToken cancellationToken
    )
    {
        await identityService.UpdateUserEmail(
            user.CognitoUsername,
            emailUpdatedEvent.NewWorkEmail,
            cancellationToken
        );
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

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "An error occur whilst updating user details."
    )]
    private partial void LogUpdatingUserDetailsFailed(Exception ex);

    public record UserInformationTrackingProjection()
    {
        public required int? UserId { get; init; }
        public required int? RegistrationRequestId { get; init; }
        public required int OrganisationId { get; internal set; }
        public required UserOrgStatus Status { get; internal set; }
        public required UserOrgMembershipStatus? MembershipStatus { get; init; }
        public required UserRole UserRole { get; internal set; }
        public required string WorkEmail { get; init; }
        public required DateTime? LastActive { get; init; }
        public required CognitoUsername? CognitoUsername { get; internal set; }
    }
}
