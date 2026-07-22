using Microsoft.EntityFrameworkCore;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.InternalServices.Communication;
using UKPS.Api.Application.InternalServices.Hosting;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Application.InternalServices.Temporal;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace UKPS.Api.Application.Users;

internal sealed partial class UserAdministrationService(
    IWebIdentityAdministrationService administerIdentityService,
    ICurrentUserInfoService currentUserInfoService,
    IEmailService emailService,
    ISetupLinkCreator setupLinkCreator,
    AppDbContext dbContext,
    IDateTimeProvider timeProvider,
    ILogger<UserAdministrationService> logger
) : IUserAdministrationService
{
    public async Task<Result<OnboardUserError>> OnboardUser(
        OnboardUserCommandDto command,
        CancellationToken cancellationToken
    )
    {
        if (currentUserInfoService.GetCurrentUserInfo().UserRole != UserRole.Super)
        {
            return Result<OnboardUserError>.Err(new OnboardUserError.NotAllowed());
        }
        UserOnboardingRecord userOnboardingRecord = await CreateNewUserOnboardingRecord(
            command.NewUserEmail,
            cancellationToken
        );
        await SendUserSignUpRequestedEmil(userOnboardingRecord, cancellationToken);

        return Result<OnboardUserError>.Ok();
    }

    private async Task<UserOnboardingRecord> CreateNewUserOnboardingRecord(
        string newUserEmail,
        CancellationToken cancellationToken
    )
    {
        await administerIdentityService.CreateNewUser(newUserEmail, cancellationToken);
        var userOnboardingRecord = new UserOnboardingRecord()
        {
            UserEmail = newUserEmail,
            SetupToken = Guid.CreateVersion7(),
            CreatedBy = currentUserInfoService.GetCurrentUserInfo().Email,
            CreatedAt = timeProvider.GetUtcNow(),
        };
        dbContext.UserOnboardingRecords.Add(userOnboardingRecord);

        await dbContext.SaveChangesAsync(cancellationToken);
        LogNewUserOnboardingRecordCreated(userOnboardingRecord.Id);
        return userOnboardingRecord;
    }

    private async Task SendUserSignUpRequestedEmil(
        UserOnboardingRecord userOnboardingRecord,
        CancellationToken cancellationToken
    )
    {
        var link = setupLinkCreator.GetSetupLink(userOnboardingRecord.SetupToken);
        await emailService.SendEmail(
            userOnboardingRecord.UserEmail,
            new UserSignUpRequestEmail() { Link = link },
            cancellationToken
        );
        LogSendingUserSignUpRequestEmail(userOnboardingRecord.Id);
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User Onboarding Record Created [Id = {Id}]."
    )]
    private partial void LogNewUserOnboardingRecordCreated(int id);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User onboarding email sent [Id = {Id}]."
    )]
    private partial void LogSendingUserSignUpRequestEmail(int id);
}
