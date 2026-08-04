using System.Diagnostics;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Options;
using UKPS.Api.Application.Authentication.Dtos;
using UKPS.Api.Application.Authentication.Errors;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Application.InternalServices.Temporal;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.Identity;
using InitiateAuthenticationResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.AuthenticationCredentialsDto,
    UKPS.Api.Application.InternalServices.Identity.InitiateAuthenticationError
>;
using SetupTokenValidationResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.Authentication.Errors.SetupTokenValidationError>;
using SetupUserResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.MultiFactorAuthenticationSetupDto,
    UKPS.Api.Application.Authentication.Errors.UserSetupError
>;
using VerifyMultiFactorAuthenticationResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.Authentication.Errors.VerifyMultiFactorAuthenticationError>;

namespace UKPS.Api.Application.Authentication;

internal class IdentityAdministrationService : IIdentityAdministrationService
{
    private readonly AppDbContext _appDbContext;
    private readonly IIdentityService _identityService;
    private readonly IOptions<UserOnboardingConfiguration> _options;
    private readonly IDateTimeProvider _dateTimeProvider;

    public IdentityAdministrationService(
        AppDbContext appDbContext,
        IIdentityService identityService,
        IOptions<UserOnboardingConfiguration> options,
        IDateTimeProvider dateTimeProvider
    )
    {
        _appDbContext = appDbContext;
        _identityService = identityService;
        _options = options;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<SetupUserResult> SetupUser(
        SetupUserCommand command,
        CancellationToken cancellationToken
    )
    {
        UserOnboardingRecord? userRecord = await _appDbContext.UserOnboardingRecords.FindAsync(
            [command.SetupToken],
            cancellationToken: cancellationToken
        );

        if (userRecord is null)
        {
            return SetupUserResult.Err(new UserSetupError.DoesNotExist());
        }

        TimeSpan timeSpan = TimeSpan.FromSeconds(_options.Value.SetupTokenExpiryTimeSeconds);
        SetupTokenState? state = userRecord.GetCurrentState(
            _dateTimeProvider.GetUtcNow(),
            timeSpan
        );

        if (state != SetupTokenState.Valid)
        {
            return state switch
            {
                SetupTokenState.Consumed => SetupUserResult.Err(new UserSetupError.Consumed()),
                SetupTokenState.Expired => SetupUserResult.Err(new UserSetupError.Expired()),
                _ => throw new UnreachableException(
                    $"Unexpected setup token state '{state}' encountered when validating setup token."
                ),
            };
        }

        userRecord.MarkAsConsumed(_dateTimeProvider.GetUtcNow());

        await _appDbContext.SaveChangesAsync(cancellationToken);

        Result<UpdatePasswordError> updatePasswordResult = await _identityService.UpdatePassword(
            userRecord.UserEmail,
            command.NewPassword,
            cancellationToken
        );

        if (updatePasswordResult.IsErr)
        {
            UserSetupError error = updatePasswordResult.Error.Match(
                invalidPassword: _ => new UserSetupError.InvalidPassword()
            );
            return SetupUserResult.Err(error);
        }

        return await InitiateAuthenticationAndGetOtp(
            userRecord.UserEmail,
            command.NewPassword,
            cancellationToken
        );
    }

    public async Task<VerifyMultiFactorAuthenticationResult> VerifyMultiFactorAuthentication(
        VerifyMultiFactorAuthenticationCommand command,
        CancellationToken cancellationToken
    )
    {
        UserOnboardingRecord? userRecord =
            await _appDbContext.UserOnboardingRecords.FindAsync(
                [command.SetupToken],
                cancellationToken: cancellationToken
            ) ?? throw new InvalidOperationException();

        try
        {
            await _identityService.VerifySoftwareToken(
                userRecord.UserEmail,
                command.AuthenticationSession,
                command.Code,
                cancellationToken
            );
            await _identityService.MarkEmailAsVerified(userRecord.UserEmail, cancellationToken);
            return VerifyMultiFactorAuthenticationResult.Ok();
        }
        catch (CodeMismatchException)
        {
            return VerifyMultiFactorAuthenticationResult.Err(
                new VerifyMultiFactorAuthenticationError.InvalidCode()
            );
        }
    }

    private async Task<SetupUserResult> InitiateAuthenticationAndGetOtp(
        string userEmail,
        string newPassword,
        CancellationToken cancellationToken
    )
    {
        InitiateAuthenticationResult initiateAuthenticationResult =
            await _identityService.InitiateAuthentication(
                userEmail,
                newPassword,
                cancellationToken
            );

        return await initiateAuthenticationResult.Match(
            _ => throw new InvalidOperationException(),
            err =>
                err.Match(
                    unauthorised: () =>
                        Task.FromResult(SetupUserResult.Err(new UserSetupError.Unauthorised())),
                    challenge: c => HandleChallenge(userEmail, c, cancellationToken)
                )
        );
    }

    private async Task<SetupUserResult> HandleChallenge(
        string userEmail,
        InitiateAuthenticationError.Challenge challenge,
        CancellationToken cancellationToken
    )
    {
        if (challenge.ChallengeType != UkpsChallengeType.MultiFactorAuthenticationSetupRequired)
        {
            throw new InvalidOperationException(
                $"Unexpected challenge [{challenge.ChallengeType}]."
            );
        }

        var result = await _identityService.AssociateSoftwareToken(
            challenge.AuthenticationSessionId,
            cancellationToken
        );

        return SetupUserResult.Ok(
            new()
            {
                OtpAuthUri = new OptAuthUri(userEmail, result.Secret).ToUri(),
                AuthenticationSession = result.AuthenticationSession,
            }
        );
    }

    public async Task<SetupTokenValidationResult> Validate(
        Guid setupToken,
        CancellationToken cancellationToken
    )
    {
        UserOnboardingRecord? userRecord = await _appDbContext.UserOnboardingRecords.FindAsync(
            [setupToken],
            cancellationToken: cancellationToken
        );

        TimeSpan timeSpan = TimeSpan.FromSeconds(_options.Value.SetupTokenExpiryTimeSeconds);
        return (userRecord?.GetCurrentState(_dateTimeProvider.GetUtcNow(), timeSpan)) switch
        {
            null => SetupTokenValidationResult.Err(new SetupTokenValidationError.DoesNotExist()),
            SetupTokenState.Expired => SetupTokenValidationResult.Err(
                new SetupTokenValidationError.Expired()
            ),
            SetupTokenState.Consumed => SetupTokenValidationResult.Err(
                new SetupTokenValidationError.Consumed()
            ),
            _ => SetupTokenValidationResult.Ok(),
        };
    }
}
