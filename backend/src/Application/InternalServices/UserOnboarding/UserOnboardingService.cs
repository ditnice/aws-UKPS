using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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

    public async Task<Result<UserOnboardingOutcome, OnboardUserError>> OnboardUserToOrganisation(
        OnboardUserCommandDto command,
        OnboardingFlow onboardingFlow,
        CancellationToken cancellationToken,
        IDbContextTransaction? externalTx = null
    )
    {
        User? existingUser = await _dbContext
            .Users.Include(x => x.UserOrgMemberships)
            .GetByEmailOrDefault(command.NewUserEmail, cancellationToken);
        if (existingUser is not null)
        {
            return await AddMembershipToExistingUser(existingUser, command, cancellationToken);
        }

        if (externalTx is not null || _dbContext.Database.CurrentTransaction != null)
        {
            return await RunUserCreationAsync(
                command,
                onboardingFlow,
                commit: null,
                cancellationToken
            );
        }

        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            return await RunUserCreationAsync(
                command,
                onboardingFlow,
                commit: () => tx.CommitAsync(cancellationToken),
                cancellationToken
            );
        });
    }

    private async Task<Result<UserOnboardingOutcome, OnboardUserError>> RunUserCreationAsync(
        OnboardUserCommandDto command,
        OnboardingFlow onboardingFlow,
        Func<Task>? commit,
        CancellationToken cancellationToken
    )
    {
        // Advisory xact lock is released when whichever transaction is in use ends.
        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtextextended(ukps.normalise_email({command.NewUserEmail}), 0))",
            cancellationToken
        );

        bool emailInUse = await _dbContext.Users.AnyAsync(
            x =>
                x.NormalisedWorkEmail == EmailDbFunctions.Normalise(command.NewUserEmail)
                || x.NormalisedPendingWorkEmail == EmailDbFunctions.Normalise(command.NewUserEmail),
            cancellationToken
        );
        if (emailInUse)
        {
            return Result<UserOnboardingOutcome, OnboardUserError>.Err(
                new OnboardUserError.UserAlreadyInOrganisation()
            );
        }

        CognitoUsername cognitoUsername = CognitoUsername.GenerateNew();
        var identityResult = await _administerIdentityService.CreateNewUser(
            cognitoUsername,
            command.NewUserEmail,
            cancellationToken
        );

        return await identityResult.Match(
            onOk: async () =>
            {
                var created = await CreateANewUserInDatabase(
                    cognitoUsername,
                    onboardingFlow,
                    command,
                    cancellationToken
                );
                if (commit != null)
                    await commit(); // only commit if we own the transaction
                return created;
            },
            onErr: HandleIdentityUserCreationFailed
        );
    }

    private static Task<
        Result<UserOnboardingOutcome, OnboardUserError>
    > HandleIdentityUserCreationFailed(CreateNewUserError error)
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
            when (ConstraintExceptions.IsUniqueEmailException(updateException))
        {
            return Result<UserOnboardingOutcome, OnboardUserError>.Err(
                new OnboardUserError.UserAlreadyInOrganisation()
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
        bool added = existingUser.TryAddMembership(
            command.OrganisationId,
            _dateTimeProvider.GetUtcNow()
        );
        if (!added)
        {
            return Result<UserOnboardingOutcome, OnboardUserError>.Err(
                new OnboardUserError.UserAlreadyInOrganisation()
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
        catch (DbUpdateException updateException)
            when (updateException.InnerException is PostgresException postgresException
                && string.Equals(
                    postgresException.ConstraintName,
                    ConstraintNames.UserMembershipUniqueUserAndOrgId,
                    StringComparison.Ordinal
                )
            )
        {
            return Result<UserOnboardingOutcome, OnboardUserError>.Err(
                new OnboardUserError.UserAlreadyInOrganisation()
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
