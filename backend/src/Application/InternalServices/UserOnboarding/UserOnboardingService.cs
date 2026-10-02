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

namespace UKPS.Api.Application.InternalServices.UserOnboarding;

internal sealed partial class UserOnboardingService
{
    private readonly AppDbContext _dbContext;
    private readonly IIdentityService _administerIdentityService;
    private readonly ICurrentUserInfoService _currentUserInfoService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<UserOnboardingService> _logger;

    public UserOnboardingService(
        AppDbContext dbContext,
        IIdentityService administerIdentityService,
        ICurrentUserInfoService currentUserInfoService,
        IDateTimeProvider dateTimeProvider,
        ILogger<UserOnboardingService> logger
    )
    {
        _dbContext = dbContext;
        _administerIdentityService = administerIdentityService;
        _currentUserInfoService = currentUserInfoService;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<Result<User, OnboardUserError>> InitialiseNewUserSetup(
        OnboardUserCommandDto command,
        CancellationToken cancellationToken
    )
    {
        // This method currently assumes that a user will only ever have one membership. This will need to be updated in the future.

        User? existingUser = await _dbContext.Users.FirstOrDefaultAsync(
            x => x.WorkEmail == command.NewUserEmail,
            cancellationToken
        );
        if (existingUser is not null)
        {
            return Result<User, OnboardUserError>.Err(
                new OnboardUserError.UserEmailAlreadyExists()
            );
        }

        Task<Result<User, OnboardUserError>> HandleIdentityUserCreationFailed(
            CreateNewUserError error
        )
        {
            return error switch
            {
                CreateNewUserError.UsernameAlreadyExists => Task.FromResult(
                    Result<User, OnboardUserError>.Err(new OnboardUserError.UsernameAlreadyExists())
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
            onOk: () => CreateANewUserInDatabase(cognitoUsername, command, cancellationToken),
            onErr: HandleIdentityUserCreationFailed
        );
    }

    private async Task<Result<User, OnboardUserError>> CreateANewUserInDatabase(
        CognitoUsername cognitoUsername,
        OnboardUserCommandDto command,
        CancellationToken cancellationToken
    )
    {
        var user = User.CreateInitialisedUser(
            new()
            {
                CognitoUsername = cognitoUsername,
                FullName = command.FullName,
                WorkEmail = command.NewUserEmail,
                WorkTelephone = command.ContactNumber,
                OrganisationId = command.OrganisationId,
                CurrentUserEmail = _currentUserInfoService.GetCurrentUserInfo().Email,
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
            return Result<User, OnboardUserError>.Err(new OnboardUserError.InvalidOrganisation());
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
            return Result<User, OnboardUserError>.Err(
                new OnboardUserError.UserEmailAlreadyExists()
            );
        }

        string sanitisedGuid = Sanitise(user.OnboardingRecord!.SetupToken);
        LogNewUserOnboardingRecordCreated(sanitisedGuid);
        return Result<User, OnboardUserError>.Ok(user);
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User Onboarding Record Created [Token = {token}...]."
    )]
    private partial void LogNewUserOnboardingRecordCreated(string token);

    private static string Sanitise(Guid guid) => guid.ToString().Substring(0, 8);
}
