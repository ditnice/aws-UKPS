using UKPS.Api.Application.Common;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.InternalServices.Communication;
using UKPS.Api.Application.InternalServices.Hosting;
using UKPS.Api.Application.InternalServices.UserOnboarding;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Application.Users.Errors;
using UKPS.Api.Persistence.Entities.Identity;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using OnboardingUserResult = UKPS.Api.Application.Common.Result<
    int,
    UKPS.Api.Application.Users.Errors.OnboardUserError
>;

namespace UKPS.Api.Application.Users;

internal sealed partial class UserAdministrationService(
    IOrganisationAuthoriser organisationAuthoriser,
    IEmailService emailService,
    ISetupLinkCreator setupLinkCreator,
    UserOnboardingService userOnboardingService,
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
        Result<User, OnboardUserError> createUserResult =
            await userOnboardingService.InitialiseNewUserSetup(command, cancellationToken);
        return await createUserResult.Match(
            onOk: async result =>
            {
                await SendUserSignUpRequestedEmail(result, cancellationToken);
                return OnboardingUserResult.Ok(result.Id);
            },
            onErr: err => Task.FromResult(OnboardingUserResult.Err(err))
        );
    }

    private async Task SendUserSignUpRequestedEmail(User user, CancellationToken cancellationToken)
    {
        Uri link = setupLinkCreator.GetSetupLink(user.OnboardingRecord!.SetupToken);
        if (user.OnboardingRecord is null)
        {
            throw new InvalidOperationException("Onboarding record was not set as expected.");
        }
        await emailService.SendEmail(
            new SendEmailCommand()
            {
                PersonIdentifier = user.CognitoUsername,
                RecipientAddress = user.WorkEmail,
                Email = new UserSignUpRequestEmail() { Link = link },
            },
            cancellationToken
        );
        string sanitisedGuid = Sanitise(user.OnboardingRecord.SetupToken);
        LogSendingUserSignUpRequestEmail(sanitisedGuid);
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User onboarding email sent [Token = {token}...]."
    )]
    private partial void LogSendingUserSignUpRequestEmail(string token);

    private static string Sanitise(Guid guid) => guid.ToString().Substring(0, 8);
}
