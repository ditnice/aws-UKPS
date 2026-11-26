using System.Diagnostics;
using System.Linq.Expressions;
using Amazon.Runtime;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using UKPS.Api.Application.Authentication;
using UKPS.Api.Application.Authentication.SecurityNotification;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.InternalServices.Communication;
using UKPS.Api.Application.InternalServices.Hosting;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Application.InternalServices.Temporal;
using UKPS.Api.Application.Organisations.Dtos;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Application.Users.Errors;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Configurations;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;
using GetCurrentOrganisationChampionsResult = UKPS.Api.Application.Common.Result<
    System.Collections.Generic.IReadOnlyCollection<string>,
    UKPS.Api.Application.Users.Errors.GetCurrentOrganisationChampionsError
>;
using GetUserInformationResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Users.Dtos.UserInformationDto,
    UKPS.Api.Application.Users.Errors.GetUsersError
>;
using GetUsersResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Common.PaginatedResponseDto<UKPS.Api.Application.Users.Dtos.UserListItemDto>,
    UKPS.Api.Application.Users.Errors.GetUsersError
>;
using RemoveUserResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Users.Dtos.RemovedUserDto,
    UKPS.Api.Application.Users.Errors.RemoveUserError
>;
using UpdateCurrentOrganisationResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.Users.Errors.UpdateCurrentOrganisationError>;
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
    CurrentDbUserEntityService currentDbUserEntityService,
    ICurrentUserInfoService currentUserInfoService,
    IEmailService emailService,
    IEmailVerificationLinkCreator emailVerificationLinkCreator,
    IOptions<AccountRecoveryOptions> accountRecoveryOptions,
    ILogger<UserService> logger
) : IUserService
{
    private TimeSpan PendingEmailChangeTimeout =>
        accountRecoveryOptions.Value.PendingEmailChangeTimeout;

    public async Task<CurrentUserInformationDto> GetCurrentUser(CancellationToken cancellationToken)
    {
        CurrentUser currentUser = currentUserInfoService.GetCurrentUserInfo();
        User user = await currentDbUserEntityService.GetCurrentUser(
            x => x.Include(x => x.UserOrgMemberships)!.ThenInclude(x => x.Organisation),
            cancellationToken
        );
        UserOrgMembership membership =
            user.UserOrgMemberships!.FirstOrDefault(x =>
                x.OrganisationId == currentUser.OrganisationId
            )
            ?? throw new InvalidOperationException(
                "Current user did not have the membership as expected."
            );

        var mfaInformation = await GetUserMfaInformation(user.CognitoUsername, cancellationToken);

        // The current user's email and role come from the session token claims,
        // as those are what the backend authorises against.
        var userInformationDto = MapUserToUserInformationDto(
            membership,
            timeProvider.GetUtcNow(),
            PendingEmailChangeTimeout
        ) with
        {
            WorkEmail = currentUser.Email,
            UserRole = currentUser.UserRole,
        };
        return new CurrentUserInformationDto(
            mfaInformation.MfaMethod,
            mfaInformation.MfaPhoneNumber,
            user.HasMultipleSelectableOrganisations(),
            userInformationDto
        );
    }

    private async Task<MfaInformation> GetUserMfaInformation(
        CognitoUsername cognitoUsername,
        CancellationToken cancellationToken
    )
    {
        string mfaMethod = "Unknown";
        string mfaPhoneNumber = string.Empty;
        try
        {
            UserMfaDetails? mfaDetails = await identityService.GetUserMfaDetails(
                cognitoUsername,
                cancellationToken
            );
            if (mfaDetails is not null)
            {
                mfaMethod = mfaDetails.MfaMethod;
                mfaPhoneNumber = mfaDetails.MfaPhoneNumber;
            }
        }
        catch (AmazonServiceException ex)
        {
            LogGettingUserMfaDetailsCognitoServiceFailed(ex);
        }
        catch (AmazonClientException ex)
        {
            LogGettingUserMfaDetailsAwsClientFailed(ex);
        }
        return new(mfaMethod, mfaPhoneNumber);
    }

    private record MfaInformation(string MfaMethod, string MfaPhoneNumber);

    public async Task<UpdateCurrentOrganisationResult> UpdateCurrentOrganisation(
        UpdateCurrentOrganisationCommand command,
        CancellationToken cancellationToken
    )
    {
        User user = await currentDbUserEntityService.GetCurrentUser(
            q => q.Include(x => x.UserOrgMemberships)!.ThenInclude(x => x.Organisation),
            cancellationToken
        );

        if (!user.CanSelectAsCurrentOrganisation(command.OrganisationId))
        {
            return UpdateCurrentOrganisationResult.Err(
                new UpdateCurrentOrganisationError.ProvidedOrganisationWasNotValid()
            );
        }

        // Disposing the transaction without committing rolls back any changes.
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            cancellationToken
        );

        // Needs to be done in two separate operations so that the unique index on the
        // selected organisation is not violated while the selection is changed.
        user.ResetCurrentOrganisation();
        await dbContext.SaveChangesAsync(cancellationToken);

        if (!user.TryUpdateCurrentOrganisation(command.OrganisationId))
        {
            throw new UnreachableException(
                "The organisation was validated as selectable but could not be selected."
            );
        }
        await dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return UpdateCurrentOrganisationResult.Ok();
    }

    public async Task<IReadOnlyCollection<OrganisationListDto>> GetCurrentUserOrganisations(
        CancellationToken cancellationToken
    )
    {
        User user = await currentDbUserEntityService.GetCurrentUser(
            q => q.Include(x => x.UserOrgMemberships)!.ThenInclude(x => x.Organisation),
            cancellationToken
        );

        return user.UserOrgMemberships!.Where(x => x.IsSelectable())
            .DistinctBy(x => x.OrganisationId)
            .Select(x => new OrganisationListDto
            {
                Id = x.OrganisationId,
                OrganisationName = x.Organisation!.OrganisationName,
            })
            .OrderBy(x => x.OrganisationName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<GetCurrentOrganisationChampionsResult> GetCurrentOrganisationChampionEmails(
        CancellationToken cancellationToken
    )
    {
        User user = await currentDbUserEntityService.GetCurrentUser(
            q => q.Include(x => x.UserOrgMemberships),
            cancellationToken
        );

        int? organisationId = user.FindCurrentOrganisationId();
        if (organisationId is null)
        {
            return GetCurrentOrganisationChampionsResult.Err(
                new GetCurrentOrganisationChampionsError.NotAllowed()
            );
        }

        UserOrgMembershipStatus[] allowedCallerStatuses =
        [
            UserOrgMembershipStatus.Active,
            UserOrgMembershipStatus.Inactive,
            UserOrgMembershipStatus.Deactivated,
        ];

        bool callerIsMember = user.UserOrgMemberships!.Any(x =>
            x.OrganisationId == organisationId && allowedCallerStatuses.Contains(x.Status)
        );

        if (!callerIsMember)
        {
            return GetCurrentOrganisationChampionsResult.Err(
                new GetCurrentOrganisationChampionsError.NotAllowed()
            );
        }

        List<string> championEmails = await dbContext
            .UserOrgMemberships.EnabledChampions(organisationId.Value)
            .Select(x => x.User!.WorkEmail)
            .Distinct()
            .OrderBy(email => email)
            .ToListAsync(cancellationToken);

        return GetCurrentOrganisationChampionsResult.Ok(championEmails);
    }

    public async Task<GetUsersResult> GetUsers(
        GetUsersQueryDto getUsersQuery,
        CancellationToken cancellationToken
    )
    {
        GetUsersError? organisationError = await ValidateOrganisationAsync(
            getUsersQuery.OrganisationId,
            Operation.ElevatedRead,
            cancellationToken
        );
        if (organisationError is not null)
        {
            return GetUsersResult.Err(organisationError);
        }

        var permittedOrganisationIds = organisationAuthoriser.GetAuthorisedOrganisations(
            Operation.ElevatedRead
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

        var membership = await dbContext
            .UserOrgMemberships.AsNoTrackingWithIdentityResolution()
            .Include(x => x.Organisation)
            .Include(x => x.User!)
                .ThenInclude(x => x.UserOrgMemberships!)
                    .ThenInclude(x => x.Organisation)
            .Where(m => m.UserId == userId && m.OrganisationId == organisationId)
            .FirstOrDefaultAsync(cancellationToken);

        if (membership is null)
        {
            return GetUserInformationResult.Err(
                new GetUsersError.UserNotFound(userId, organisationId)
            );
        }
        var dto = MapUserToUserInformationDto(
            membership,
            timeProvider.GetUtcNow(),
            PendingEmailChangeTimeout
        );

        return GetUserInformationResult.Ok(dto);
    }

    private static UserInformationDto MapUserToUserInformationDto(
        UserOrgMembership membership,
        DateTime currentTime,
        TimeSpan pendingEmailChangeTimeout
    )
    {
        User user = membership.User!;
        return new UserInformationDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            WorkTelephone = user.WorkTelephone ?? string.Empty,
            WorkEmail = user.WorkEmail,
            OrganisationMembershipId = membership.Id,
            OrganisationId = membership.Organisation!.Id,
            OrganisationName = membership.Organisation.OrganisationName,
            UserRole = membership.UserRole,
            Status = membership.Status.ConvertToUserOrgStatus(),
            AllowedPharmaceuticalEntity = membership.AllowedPharmaceuticalEntity,
            PendingWorkEmail = user
                .PendingEmail.TryGetData(currentTime, pendingEmailChangeTimeout)
                .TryGetSuccessValue(out var data)
                ? new() { Value = data.WorkEmail, VerifiedByEmail = data.Verified }
                : null,
        };
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

        IQueryable<UserRegistrationRequest> activeMostRecentValues =
            GetMostRecentActiveRegistrationRequests();
        var userRegistrationRequestsProjections = activeMostRecentValues.Select(x => new
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

    private IQueryable<UserRegistrationRequest> GetMostRecentActiveRegistrationRequests()
    {
        IQueryable<UserRegistrationRequest> mostRecentValues =
            dbContext.UserRegistrationRequests.Where(x =>
                x.CreatedAt
                == dbContext
                    .UserRegistrationRequests.Where(y =>
                        y.WorkEmail == x.WorkEmail && y.OrganisationId == x.OrganisationId
                    )
                    .Max(y => y.CreatedAt)
            );
        IQueryable<UserRegistrationRequest> activeMostRecentValues = mostRecentValues.Where(x =>
            x.RejectedAt == null && x.ApprovedAt == null
        );
        return activeMostRecentValues;
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
            return UserOrgMembership
                .GetPermittedActions(m.MembershipStatus.Value)
                .Where(action =>
                    action != UserMembershipAction.RemoveUser
                    || currentUserInfo.UserRole == UserRole.Super
                )
                .ToArray();
        }

        throw new InvalidOperationException(
            "UserInformationTrackingProjection was in an invalid state"
        );
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
            // No specific organisation requested, so the caller must be permitted the
            // operation on at least one organisation.
            return organisationAuthoriser.GetAuthorisedOrganisations(operation).IsNone
                ? new GetUsersError.NotAllowed(null)
                : null;
        }

        currentUserInfoService.ThrowIfOrganisationNotSelected(organisationId.Value);

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
            string pattern = $"%{Helpers.EscapeLikePattern(getUsersQuery.Email)}%";
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
                user
            );

            if (!isTheCurrentUserModifyingTheirOwnDetails)
            {
                return UpdateUserDetailsResult.Err(new UpdateUserDetailsError.Unauthorised());
            }

            var validationError = ValidateUserDetails(command);
            if (validationError is not null)
            {
                return UpdateUserDetailsResult.Err(validationError);
            }

            user.UpdateDetails(
                command.Property == EditableUserDetail.FullName ? command.Value : user.FullName,
                command.Property == EditableUserDetail.WorkPhone
                    ? command.Value
                    : user.WorkTelephone,
                timeProvider.GetUtcNow()
            );

            await dbContext.SaveChangesAsync(cancellationToken);
            await HandleUserEvents(user, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            UserDetailsDto output = MapToDto(user);
            return UpdateUserDetailsResult.Ok(output);
        }
        catch (Exception ex)
        {
            LogUpdatingUserDetailsFailed(ex);
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<UpdateUserEmailError>> UpdateUserEmail(
        int userId,
        UpdateUserEmailCommand command,
        CancellationToken cancellationToken
    )
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(
                cancellationToken
            );

            try
            {
                // Advisory xact lock is released when whichever transaction is in use ends.
                await dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT pg_advisory_xact_lock(hashtextextended(ukps.normalise_email({command.Value}), 0))",
                    cancellationToken
                );

                User? user = await dbContext.Users.FindAsync([userId], cancellationToken);

                var error = await ValidateEmailUpdatePermitted(user, command, cancellationToken);
                if (error is not null)
                {
                    return Result<UpdateUserEmailError>.Err(error);
                }

                user!.UpdateEmail(
                    command.Value,
                    VerificationTokenGenerator.Generate(),
                    timeProvider.GetUtcNow()
                );

                await dbContext.SaveChangesAsync(cancellationToken);
                await HandleUserEvents(user, cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return Result<UpdateUserEmailError>.Ok();
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<UpdateUserEmailError>.Err(
                    new UpdateUserEmailError.ConcurrentChangeDetected()
                );
            }
            catch (DbUpdateException updateException)
                when (ConstraintExceptions.IsUniquePendingEmailException(updateException))
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<UpdateUserEmailError>.Err(
                    new UpdateUserEmailError.ConflictingEmail()
                );
            }
            catch (Exception ex)
            {
                LogUpdatingUserDetailsFailed(ex);
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    public async Task<UpdateEmailToCompletePendingEmailChangeResult> UpdateEmailToCompletePendingEmailChange(
        int userId,
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
                return UpdateEmailToCompletePendingEmailChangeResult.Err(
                    new UpdateEmailToCompletePendingEmailError.NotFound()
                );
            }

            var result = user.CompletePendingEmail(
                timeProvider.GetUtcNow(),
                PendingEmailChangeTimeout
            );

            return await result.Match(
                async _ =>
                {
                    await dbContext.SaveChangesAsync(cancellationToken);
                    await HandleUserEvents(user, cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return UpdateEmailToCompletePendingEmailChangeResult.Ok();
                },
                err =>
                    Task.FromResult(
                        UpdateEmailToCompletePendingEmailChangeResult.Err(
                            MapEmailCompletionError(err)
                        )
                    )
            );
        }
        catch (DbUpdateException updateException)
            when (ConstraintExceptions.IsUniqueEmailException(updateException))
        {
            await transaction.RollbackAsync(cancellationToken);
            return UpdateEmailToCompletePendingEmailChangeResult.Err(
                new UpdateEmailToCompletePendingEmailError.ConflictingEmail()
            );
        }
    }

    private static UpdateEmailToCompletePendingEmailError MapEmailCompletionError(
        PendingEmailChange.CompletionError err
    )
    {
        return err.Match<UpdateEmailToCompletePendingEmailError>(
            noPendingEmailToComplete: _ => new UpdateEmailToCompletePendingEmailError.NotFound(),
            notVerified: _ => new UpdateEmailToCompletePendingEmailError.NotVerified(),
            pendingEmailTimedOut: _ => new UpdateEmailToCompletePendingEmailError.PendingEmailExpired()
        );
    }

    public async Task<ResendVerificationEmailResult> ResendVerificationEmail(
        int userId,
        CancellationToken cancellationToken
    )
    {
        User? user = await dbContext.Users.FindAsync([userId], cancellationToken);

        if (user is null)
        {
            return ResendVerificationEmailResult.Err(new ResendVerificationEmailError.NotFound());
        }

        if (!currentUserInfoService.IsCurrentUser(user))
        {
            return ResendVerificationEmailResult.Err(new ResendVerificationEmailError.Forbidden());
        }

        var token = VerificationTokenGenerator.Generate();
        var result = user.TryRefreshVerificationToken(
            token,
            timeProvider.GetUtcNow(),
            PendingEmailChangeTimeout
        );

        return await result.Match<Task<ResendVerificationEmailResult>>(
            async (pendingWorkEmail) =>
            {
                try
                {
                    await dbContext.SaveChangesAsync(cancellationToken);
                    await SendEmailVerificationEmail(
                        pendingWorkEmail,
                        token,
                        user.CognitoUsername,
                        cancellationToken
                    );
                }
                catch (DbUpdateConcurrencyException)
                {
                    return ResendVerificationEmailResult.Err(
                        new ResendVerificationEmailError.ConcurrencyDetected()
                    );
                }
                return ResendVerificationEmailResult.Ok();
            },
            err =>
                Task.FromResult(
                    ResendVerificationEmailResult.Err(MapRefreshVerificationTokenError(err))
                )
        );
    }

    private static ResendVerificationEmailError MapRefreshVerificationTokenError(
        PendingEmailChange.RefreshVerificationTokenError err
    )
    {
        return err.Match<ResendVerificationEmailError>(
            noPendingEmailToRefresh: _ => new ResendVerificationEmailError.NoPendingVerification(),
            alreadyVerified: _ => new ResendVerificationEmailError.PendingEmailAlreadyVerified(),
            // An expired change can no longer be verified, so there is nothing to resend.
            pendingEmailTimedOut: _ => new ResendVerificationEmailError.NoPendingVerification()
        );
    }

    public async Task<VerifyPendingEmailAddressResult> VerifyPendingEmailAddress(
        int userId,
        VerifyPendingEmailAddressCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            User? user = await dbContext.Users.FindAsync([userId], cancellationToken);

            var error = ValidatePendingEmailVerificationPermitted(user);
            if (error is not null)
            {
                return VerifyPendingEmailAddressResult.Err(error);
            }

            var result = user!.TryMarkPendingEmailAsVerified(
                command.VerificationToken,
                timeProvider.GetUtcNow(),
                PendingEmailChangeTimeout
            );
            return await result.Match<Task<VerifyPendingEmailAddressResult>>(
                async () =>
                {
                    await dbContext.SaveChangesAsync(cancellationToken);
                    await HandleUserEvents(user, cancellationToken);

                    return VerifyPendingEmailAddressResult.Ok();
                },
                err =>
                    Task.FromResult(
                        VerifyPendingEmailAddressResult.Err(MapPendingEmailAsVerifiedError(err))
                    )
            );
        }
        catch (DbUpdateConcurrencyException)
        {
            return VerifyPendingEmailAddressResult.Err(
                new VerifyPendingEmailAddressError.ConcurrentChangeDetected()
            );
        }
        catch (Exception ex)
        {
            LogUpdatePendingEmailAsVerifiedFailed(ex);
            throw;
        }
    }

    private static VerifyPendingEmailAddressError MapPendingEmailAsVerifiedError(
        PendingEmailChange.VerificationError error
    )
    {
        return error.Match<VerifyPendingEmailAddressError>(
            noPendingEmailToVerify: _ => new VerifyPendingEmailAddressError.NoPendingEmailToVerify(),
            invalidVerificationToken: _ => new VerifyPendingEmailAddressError.InvalidTokenProvided(),
            alreadyVerified: _ => new VerifyPendingEmailAddressError.EmailAlreadyVerified(),
            pendingEmailTimedOut: _ => new VerifyPendingEmailAddressError.PendingEmailExpired()
        );
    }

    public async Task<Result<CancelPendingEmailChangeError>> CancelPendingEmailChange(
        int userId,
        CancellationToken cancellationToken
    )
    {
        try
        {
            User? user = await dbContext.Users.FindAsync([userId], cancellationToken);

            if (user is null)
            {
                return Result<CancelPendingEmailChangeError>.Err(
                    new CancelPendingEmailChangeError.UserDoesNotExist()
                );
            }

            if (!currentUserInfoService.IsCurrentUser(user))
            {
                return Result<CancelPendingEmailChangeError>.Err(
                    new CancelPendingEmailChangeError.Unauthorised()
                );
            }

            var result = user.TryCancelPendingEmailChange(
                timeProvider.GetUtcNow(),
                PendingEmailChangeTimeout
            );
            return await result.Match(
                async () =>
                {
                    await dbContext.SaveChangesAsync(cancellationToken);
                    return Result<CancelPendingEmailChangeError>.Ok();
                },
                async err =>
                {
                    // An expired change is no longer pending, so there is nothing to cancel.
                    return Result<CancelPendingEmailChangeError>.Err(
                        err.Match<CancelPendingEmailChangeError>(
                            noPendingEmailToCancel: _ => new CancelPendingEmailChangeError.NoPendingEmailToCancel(),
                            pendingEmailTimedOut: _ => new CancelPendingEmailChangeError.NoPendingEmailToCancel()
                        )
                    );
                }
            );
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<CancelPendingEmailChangeError>.Err(
                new CancelPendingEmailChangeError.ConcurrentChangeDetected()
            );
        }
    }

    private VerifyPendingEmailAddressError? ValidatePendingEmailVerificationPermitted(User? user)
    {
        if (user is null)
        {
            return new VerifyPendingEmailAddressError.UserNotFound();
        }

        bool isTheCurrentUserModifyingTheirOwnDetails = currentUserInfoService.IsCurrentUser(user);
        if (!isTheCurrentUserModifyingTheirOwnDetails)
        {
            return new VerifyPendingEmailAddressError.Unauthorised();
        }

        return null;
    }

    public async Task<RemoveUserResult> RemoveUser(int userId, CancellationToken cancellationToken)
    {
        CurrentUser currentUser = currentUserInfoService.GetCurrentUserInfo();

        // Removal is global across all organisations, so only Super users may perform it.
        if (currentUser.UserRole is not UserRole.Super)
        {
            return RemoveUserResult.Err(new RemoveUserError.NotAllowed(userId));
        }

        await using IDbContextTransaction transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            User? user = await dbContext.Users.FindAsync([userId], cancellationToken);

            if (user is null)
            {
                return RemoveUserResult.Err(new RemoveUserError.UserNotFound(userId));
            }

            // Compare the stable Cognito identity so email changes cannot bypass the self-removal guard.
            if (user.CognitoUsername == currentUser.CognitoUsername)
            {
                return RemoveUserResult.Err(new RemoveUserError.CannotRemoveSelf(userId));
            }

            RemoveUserError? membershipError = await RemoveMemberships(userId, cancellationToken);
            if (membershipError is not null)
            {
                return RemoveUserResult.Err(membershipError);
            }

            await AnonymiseUserAndAuditHistory(user, currentUser, cancellationToken);
            // A Cognito failure rolls back the DB changes. If deletion succeeds but commit fails,
            // DB changes roll back and retry tolerates the absent Cognito user. Committing first
            // blocks endpoint retries because Removed is terminal; reordering needs durable retries.
            await identityService.DeleteUser(user.CognitoUsername, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return RemoveUserResult.Ok(new RemovedUserDto { DisplayName = user.FullName });
        }
        catch (Exception ex)
        {
            LogRemovingUserFailed(ex);
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static UpdateUserDetailsError.ValidationError? ValidateUserDetails(
        UpdateUserDetailsCommand command
    )
    {
        if (
            command.Property == EditableUserDetail.WorkPhone
            && !PhoneNumberValidator.IsValid(command.Value)
        )
        {
            return new UpdateUserDetailsError.ValidationError(
                "Value must be a valid phone number."
            );
        }
        return null;
    }

    private async Task<RemoveUserError?> RemoveMemberships(
        int userId,
        CancellationToken cancellationToken
    )
    {
        List<UserOrgMembership> memberships = await dbContext
            .UserOrgMemberships.Where(m => m.UserId == userId)
            .ToListAsync(cancellationToken);

        if (await WouldRemoveLastChampion(memberships, cancellationToken))
        {
            return new RemoveUserError.LastRemainingChampion(userId);
        }

        foreach (UserOrgMembership membership in memberships)
        {
            var transitionResult = membership.TryRemove();
            if (!transitionResult.Success)
            {
                return new RemoveUserError.NotAllowedInCurrentState(
                    transitionResult.ConvertToUserOrgStatusTransitionResult()
                );
            }
        }

        return null;
    }

    private async Task<UpdateUserEmailError?> ValidateEmailUpdatePermitted(
        User? user,
        UpdateUserEmailCommand command,
        CancellationToken cancellationToken
    )
    {
        if (user is null)
        {
            return new UpdateUserEmailError.UserDoesNotExist();
        }

        bool isTheCurrentUserModifyingTheirOwnDetails = currentUserInfoService.IsCurrentUser(user);

        if (!isTheCurrentUserModifyingTheirOwnDetails)
        {
            return new UpdateUserEmailError.Unauthorised();
        }

        bool emailHasNotChanged = command.Value.Equals(user.WorkEmail, StringComparison.Ordinal);
        if (emailHasNotChanged)
        {
            return new UpdateUserEmailError.MatchesCurrentEmail();
        }

        IQueryable<User> otherUsers = dbContext.Users.Where(x => x.Id != user.Id);
        bool emailAlreadyExistInTheDatabase = await otherUsers.AnyAsync(
            x =>
                x.NormalisedPendingWorkEmail == EmailDbFunctions.Normalise(command.Value)
                || x.NormalisedWorkEmail == EmailDbFunctions.Normalise(command.Value),
            cancellationToken
        );

        if (emailAlreadyExistInTheDatabase)
        {
            return new UpdateUserEmailError.ConflictingEmail();
        }

        return null;
    }

    private async Task<bool> WouldRemoveLastChampion(
        IReadOnlyCollection<UserOrgMembership> memberships,
        CancellationToken cancellationToken
    )
    {
        // Check every affected organisation before changing any memberships or user data.
        // This runs one query per organisation the user is a champion of. A user is rarely a
        // champion of many organisations, so a single combined query isn't worth the complexity.
        int[] removedMembershipIds = memberships.Select(m => m.Id).ToArray();
        var championOrganisationIds = memberships
            .Where(m => m.IsEnabledChampion())
            .Select(m => m.OrganisationId)
            .Distinct();

        foreach (int organisationId in championOrganisationIds)
        {
            bool hasAnotherChampion = await dbContext.UserOrgMemberships.AnyOtherEnabledChampion(
                organisationId,
                removedMembershipIds,
                cancellationToken
            );

            if (!hasAnotherChampion)
            {
                return true;
            }
        }

        return false;
    }

    private async Task AnonymiseUserAndAuditHistory(
        User user,
        CurrentUser currentUser,
        CancellationToken cancellationToken
    )
    {
        DateTime now = timeProvider.GetUtcNow();
        user.Anonymise(now);
        List<UserRegistrationRequest> registrationRequests = await dbContext
            .UserRegistrationRequests.Where(x =>
                x.ResultingUserId == user.Id && x.ApprovedAt != null && x.RejectedAt == null
            )
            .ToListAsync(cancellationToken);
        foreach (UserRegistrationRequest registrationRequest in registrationRequests)
        {
            registrationRequest.Anonymise(user.Id);
        }

        UserOnboardingRecord? onboardingRecord = await dbContext
            .UserOnboardingRecords.Where(x => x.ResultingUserId == user.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (onboardingRecord is not null)
        {
            dbContext.UserOnboardingRecords.Remove(onboardingRecord);
        }

        await AddUserRemovedAudit(user.Id, currentUser, now, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await AnonymiseUserAuditHistory(user, cancellationToken);
    }

    private async Task AddUserRemovedAudit(
        int removedUserId,
        CurrentUser currentUser,
        DateTime now,
        CancellationToken cancellationToken
    )
    {
        int currentUserId = await dbContext
            .Users.Where(u => u.CognitoUsername == currentUser.CognitoUsername)
            .Select(u => u.Id)
            .FirstAsync(cancellationToken);

        dbContext.UserAudits.Add(
            new UserAudit
            {
                UserId = removedUserId,
                EventType = IamEventType.Deleted,
                UpdatedBy = currentUserId,
                UpdatedAt = now,
            }
        );
    }

    private async Task AnonymiseUserAuditHistory(User user, CancellationToken cancellationToken)
    {
        await dbContext
            .UserAudits.Where(x =>
                x.UserId == user.Id
                && x.FieldPath != null
                && UserAuditFieldPaths.PersonalFields.Contains(x.FieldPath)
            )
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(x => x.OldValue, (string?)null)
                        .SetProperty(x => x.NewValue, (string?)null),
                cancellationToken
            );
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
                case User.PendingEmailUpdatedEvent pendingEmailUpdatedEvent:
                    await HandleUserEvent(user, pendingEmailUpdatedEvent, cancellationToken);
                    break;
            }
        }
    }

    private Task HandleUserEvent(
        User user,
        User.PendingEmailUpdatedEvent pendingEmailUpdatedEvent,
        CancellationToken cancellationToken
    )
    {
        return SendEmailVerificationEmail(
            pendingEmailUpdatedEvent.RequestedEmail,
            pendingEmailUpdatedEvent.VerificationToken,
            user.CognitoUsername,
            cancellationToken
        );
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
        await emailService.SendEmail(
            new SendEmailCommand()
            {
                Email = new SecurityNotificationEmail() { ChangedValue = ChangedUserData.Email },
                PersonIdentifier = user.CognitoUsername,
                RecipientAddress = emailUpdatedEvent.PreviousWorkEmail,
            },
            cancellationToken
        );
    }

    private async Task SendEmailVerificationEmail(
        string newEmailAddress,
        string verificationToken,
        CognitoUsername cognitoUsername,
        CancellationToken cancellationToken
    )
    {
        var link = emailVerificationLinkCreator.GetVerificationLink(verificationToken);
        await emailService.SendEmail(
            new SendEmailCommand()
            {
                RecipientAddress = newEmailAddress,
                PersonIdentifier = cognitoUsername,
                Email = new EmailChangeVerificationRequestEmail() { EmailVerificationLink = link },
            },
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

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "An error occur whilst updating user details."
    )]
    private partial void LogUpdatingUserDetailsFailed(Exception ex);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "An error occur whilst attempting to mark the pending email as verified."
    )]
    private partial void LogUpdatePendingEmailAsVerifiedFailed(Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = "An error occurred whilst removing a user.")]
    private partial void LogRemovingUserFailed(Exception ex);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Cognito service error retrieving MFA details."
    )]
    private partial void LogGettingUserMfaDetailsCognitoServiceFailed(Exception ex);

    [LoggerMessage(Level = LogLevel.Warning, Message = "AWS client error retrieving MFA details.")]
    private partial void LogGettingUserMfaDetailsAwsClientFailed(Exception ex);

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
