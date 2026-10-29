using Microsoft.EntityFrameworkCore;
using Npgsql;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Application.InternalServices.Temporal;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Application.Users.Errors;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Configurations;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.InternalServices.UserOnboarding;

internal sealed partial class UserOnboardingService
{
    private readonly AppDbContext _dbContext;
    private readonly IIdentityService _administerIdentityService;
    private readonly CurrentDbUserEntityService _currentDbUserEntityService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<UserOnboardingService> _logger;

    public UserOnboardingService(
        AppDbContext dbContext,
        IIdentityService administerIdentityService,
        CurrentDbUserEntityService currentDbUserEntityService,
        IDateTimeProvider dateTimeProvider,
        ILogger<UserOnboardingService> logger
    )
    {
        _dbContext = dbContext;
        _administerIdentityService = administerIdentityService;
        _currentDbUserEntityService = currentDbUserEntityService;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<Result<UserOnboardingOutcome, OnboardUserError>> InitialiseNewUserSetup(
        OnboardUserCommandDto command,
        OnboardingFlow onboardingFlow,
        CancellationToken cancellationToken
    )
    {
        User? existingUser = await _dbContext
            .Users.Include(x => x.UserOrgMemberships)
            .FirstOrDefaultAsync(x => x.WorkEmail == command.NewUserEmail, cancellationToken);
        if (existingUser is not null)
        {
            return await AddMembershipToExistingUser(existingUser, command, cancellationToken);
        }

        Task<Result<UserOnboardingOutcome, OnboardUserError>> HandleIdentityUserCreationFailed(
            CreateNewUserError error
        )
        {
            return error switch
            {
                CreateNewUserError.UsernameAlreadyExists => Task.FromResult(
                    Result<UserOnboardingOutcome, OnboardUserError>.Err(
                        new OnboardUserError.UsernameAlreadyExists()
                    )
                ),
                _ => throw new InvalidOperationException(
                    $"An unexpected error occurred when creating a new user [{error}]"
                ),
            };
        }

        CognitoUsername cognitoUsername = CognitoUsername.GenerateNew();
        var result = await _administerIdentityService.CreateNewUser(
            cognitoUsername,
            command.NewUserEmail,
            cancellationToken
        );
        return await result.Match(
            onOk: () =>
                CreateANewUserInDatabase(
                    cognitoUsername,
                    onboardingFlow,
                    command,
                    cancellationToken
                ),
            onErr: HandleIdentityUserCreationFailed
        );
    }

    private async Task<Result<UserOnboardingOutcome, OnboardUserError>> CreateANewUserInDatabase(
        CognitoUsername cognitoUsername,
        OnboardingFlow onboardingFlow,
        OnboardUserCommandDto command,
        CancellationToken cancellationToken
    )
    {
        User currentUser = await _currentDbUserEntityService.GetCurrentUser(cancellationToken);
        var user = User.CreateInitialisedUser(
            new()
            {
                OnboardingFlow = onboardingFlow,
                CognitoUsername = cognitoUsername,
                FullName = command.FullName,
                WorkEmail = command.NewUserEmail,
                WorkTelephone = command.ContactNumber,
                OrganisationId = command.OrganisationId,
                CurrentUserId = currentUser.Id,
                Now = _dateTimeProvider.GetUtcNow(),
            }
        );
        _dbContext.Add(user);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException updateException)
            when (updateException.InnerException is PostgresException postgresException
                && string.Equals(
                    postgresException.ConstraintName,
                    ConstraintNames.UserMembershipRequiresOrganisation,
                    StringComparison.Ordinal
                )
            )
        {
            return Result<UserOnboardingOutcome, OnboardUserError>.Err(
                new OnboardUserError.InvalidOrganisation()
            );
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
            return Result<UserOnboardingOutcome, OnboardUserError>.Err(
                new OnboardUserError.UserEmailAlreadyExists()
            );
        }

        string sanitisedGuid = Sanitise(user.OnboardingRecord!.SetupToken);
        LogNewUserOnboardingRecordCreated(sanitisedGuid);
        return Result<UserOnboardingOutcome, OnboardUserError>.Ok(
            new UserOnboardingOutcome(user, true)
        );
    }

    private async Task<Result<UserOnboardingOutcome, OnboardUserError>> AddMembershipToExistingUser(
        User existingUser,
        OnboardUserCommandDto command,
        CancellationToken cancellationToken
    )
    {
        UserOrgMembershipStatus status = existingUser.IsSetupPending()
            ? UserOrgMembershipStatus.AwaitingSetup
            : UserOrgMembershipStatus.Active;

        bool added = existingUser.TryAddMembership(
            command.OrganisationId,
            status,
            _dateTimeProvider.GetUtcNow()
        );
        if (!added)
        {
            return Result<UserOnboardingOutcome, OnboardUserError>.Err(
                new OnboardUserError.UserEmailAlreadyExists()
            );
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException updateException)
            when (updateException.InnerException is PostgresException postgresException
                && string.Equals(
                    postgresException.ConstraintName,
                    ConstraintNames.UserMembershipRequiresOrganisation,
                    StringComparison.Ordinal
                )
            )
        {
            return Result<UserOnboardingOutcome, OnboardUserError>.Err(
                new OnboardUserError.InvalidOrganisation()
            );
        }

        return Result<UserOnboardingOutcome, OnboardUserError>.Ok(
            new UserOnboardingOutcome(existingUser, false)
        );
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User Onboarding Record Created [Token = {token}...]."
    )]
    private partial void LogNewUserOnboardingRecordCreated(string token);

    private static string Sanitise(Guid guid) => guid.ToString().Substring(0, 8);
}
