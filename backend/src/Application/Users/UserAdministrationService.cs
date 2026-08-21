using Microsoft.EntityFrameworkCore;
using Npgsql;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.InternalServices.Communication;
using UKPS.Api.Application.InternalServices.Hosting;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Application.InternalServices.Temporal;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Application.Users.Errors;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Configurations;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using OnboardingUserResult = UKPS.Api.Application.Common.Result<
    int,
    UKPS.Api.Application.Users.Errors.OnboardUserError
>;

namespace UKPS.Api.Application.Users;

internal sealed partial class UserAdministrationService(
    IOrganisationAuthoriser organisationAuthoriser,
    IIdentityService administerIdentityService,
    ICurrentUserInfoService currentUserInfoService,
    IEmailService emailService,
    ISetupLinkCreator setupLinkCreator,
    AppDbContext dbContext,
    IDateTimeProvider timeProvider,
    ILogger<UserAdministrationService> logger
) : IUserAdministrationService
{
    public async Task<OnboardingUserResult> OnboardUser(
        OnboardUserCommandDto command,
        CancellationToken cancellationToken
    )
    {
        var authorised = organisationAuthoriser.CanPerformOperationOnOrganisation(
            Operation.SignUpUser,
            command.OrganisationId
        );
        if (!authorised)
        {
            return OnboardingUserResult.Err(new OnboardUserError.NotAllowed());
        }
        Result<User, OnboardUserError> createUserResult = await CreateNewUserOnboardingRecord(
            command,
            cancellationToken
        );
        return await createUserResult.Match(
            async result =>
            {
                await SendUserSignUpRequestedEmail(result, cancellationToken);
                return OnboardingUserResult.Ok(result.Id);
            },
            err => Task.FromResult(OnboardingUserResult.Err(err))
        );
    }

    private async Task<Result<User, OnboardUserError>> CreateNewUserOnboardingRecord(
        OnboardUserCommandDto command,
        CancellationToken cancellationToken
    )
    {
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

        var result = await administerIdentityService.CreateNewUser(
            command.NewUserEmail,
            cancellationToken
        );
        return await result.Match(
            x => CreateANewUserInDatabase(x, command, cancellationToken),
            HandleIdentityUserCreationFailed
        );
    }

    private async Task<Result<User, OnboardUserError>> CreateANewUserInDatabase(
        string identityId,
        OnboardUserCommandDto command,
        CancellationToken cancellationToken
    )
    {
        var userOnboardingRecord = new UserOnboardingRecord()
        {
            SetupToken = Guid.CreateVersion7(),
            CreatedBy = currentUserInfoService.GetCurrentUserInfo().Email,
            CreatedAt = timeProvider.GetUtcNow(),
        };
        var membership = new UserOrgMembership()
        {
            Status = UserOrgStatus.AwaitingSetup,
            AllowedPharmaceuticalEntity = PharmaceuticalEntity.Both, // URP 435 - Decide what initial value should be set.
            UserRole = UserRole.Standard,
            CreatedAt = timeProvider.GetUtcNow(),
            OrganisationId = command.OrganisationId,
        };
        var user = new User()
        {
            IdentityId = identityId,
            FullName = command.FullName,
            WorkEmail = command.NewUserEmail,
            WorkTelephone = command.ContactNumber,
            OnboardingRecord = userOnboardingRecord,
            CreatedAt = timeProvider.GetUtcNow(),
            UserOrgMemberships = [membership],
        };
        dbContext.Add(user);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
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

        string sanitisedGuid = Sanitise(userOnboardingRecord.SetupToken);
        LogNewUserOnboardingRecordCreated(sanitisedGuid);
        return Result<User, OnboardUserError>.Ok(user);
    }

    private async Task SendUserSignUpRequestedEmail(User user, CancellationToken cancellationToken)
    {
        Uri link = setupLinkCreator.GetSetupLink(user.OnboardingRecord!.SetupToken);
        if (user.OnboardingRecord is null)
        {
            throw new InvalidOperationException("Onboarding record was not set as expected.");
        }
        await emailService.SendEmail(
            user.WorkEmail,
            new UserSignUpRequestEmail() { Link = link },
            cancellationToken
        );
        string sanitisedGuid = Sanitise(user.OnboardingRecord.SetupToken);
        LogSendingUserSignUpRequestEmail(sanitisedGuid);
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User Onboarding Record Created [Token = {token}...]."
    )]
    private partial void LogNewUserOnboardingRecordCreated(string token);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User onboarding email sent [Token = {token}...]."
    )]
    private partial void LogSendingUserSignUpRequestEmail(string token);

    private static string Sanitise(Guid guid) => guid.ToString().Substring(0, 8);
}
