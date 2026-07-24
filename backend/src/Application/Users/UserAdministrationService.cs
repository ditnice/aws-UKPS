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
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using OnboardingUserResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.Users.Errors.OnboardUserError>;

namespace UKPS.Api.Application.Users;

internal sealed partial class UserAdministrationService(
    IOrganisationAuthoriser organisationAuthoriser,
    IWebIdentityAdministrationService administerIdentityService,
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
        Result<UserOnboardingRecord, OnboardUserError> userOnboardingRecord =
            await CreateNewUserOnboardingRecord(command, cancellationToken);

        if (userOnboardingRecord.IsErr)
        {
            return OnboardingUserResult.Err(userOnboardingRecord.Error);
        }
        await SendUserSignUpRequestedEmail(userOnboardingRecord.Value!, cancellationToken);

        return OnboardingUserResult.Ok();
    }

    private async Task<
        Result<UserOnboardingRecord, OnboardUserError>
    > CreateNewUserOnboardingRecord(
        OnboardUserCommandDto command,
        CancellationToken cancellationToken
    )
    {
        await administerIdentityService.CreateNewUser(command.NewUserEmail, cancellationToken);
        var userOnboardingRecord = new UserOnboardingRecord()
        {
            UserEmail = command.NewUserEmail,
            NewUserOrganisationId = command.OrganisationId,
            SetupToken = Guid.CreateVersion7(),
            CreatedBy = currentUserInfoService.GetCurrentUserInfo().Email,
            CreatedAt = timeProvider.GetUtcNow(),
        };
        dbContext.UserOnboardingRecords.Add(userOnboardingRecord);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException updateException)
            when (updateException.InnerException is PostgresException postgresException
                && string.Equals(
                    postgresException.ConstraintName,
                    ConstraintNames.UserOnboardingRequiresOrganisation,
                    StringComparison.Ordinal
                )
            )
        {
            return Result<UserOnboardingRecord, OnboardUserError>.Err(
                new OnboardUserError.InvalidOrganisation()
            );
        }

        string sanitisedGuid = Sanitise(userOnboardingRecord.SetupToken);
        LogNewUserOnboardingRecordCreated(sanitisedGuid);
        return Result<UserOnboardingRecord, OnboardUserError>.Ok(userOnboardingRecord);
    }

    private async Task SendUserSignUpRequestedEmail(
        UserOnboardingRecord userOnboardingRecord,
        CancellationToken cancellationToken
    )
    {
        string link = setupLinkCreator.GetSetupLink(userOnboardingRecord.SetupToken);
        await emailService.SendEmail(
            userOnboardingRecord.UserEmail,
            new UserSignUpRequestEmail() { Link = link },
            cancellationToken
        );
        string sanitisedGuid = Sanitise(userOnboardingRecord.SetupToken);
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
