using System.Diagnostics;
using Microsoft.Extensions.Options;
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
using SetupTokenValidationResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.AuthorisationAdministration.SetupTokenValidationError>;
using SetupUserResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.AuthorisationAdministration.MultiFactorAuthenticationSetupDto,
    UKPS.Api.Application.AuthorisationAdministration.UserSetupError
>;

namespace UKPS.Api.Application.AuthorisationAdministration;

internal class AuthorisationAdministrationService : IAuthorisationAdministrationService
{
    private readonly AppDbContext _appDbContext;
    private readonly CognitoWebIdentityAdministrationService _webIdentityAdministrationService;
    private readonly IOptions<UserOnboardingConfiguration> _options;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AuthorisationAdministrationService(
        AppDbContext appDbContext,
        CognitoWebIdentityAdministrationService webIdentityAdministrationService,
        IOptions<UserOnboardingConfiguration> options,
        IDateTimeProvider dateTimeProvider
    )
    {
        _appDbContext = appDbContext;
        _webIdentityAdministrationService = webIdentityAdministrationService;
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

        Result<UpdatePasswordError> updatePasswordResult =
            await _webIdentityAdministrationService.UpdatePassword(
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

        Uri otpAuthUri = await InitiateAuthenticationAndGetOtp(
            userRecord.UserEmail,
            command.NewPassword,
            cancellationToken
        );

        return SetupUserResult.Ok(new() { OtpAuthUri = otpAuthUri });
    }

    private async Task<Uri> InitiateAuthenticationAndGetOtp(
        string userEmail,
        string newPassword,
        CancellationToken cancellationToken
    )
    {
        InitiateAuthenticationResult initiateAuthenticationResult =
            await _webIdentityAdministrationService.InitiateAuthentication(
                userEmail,
                newPassword,
                cancellationToken
            );

        var challenge =
            initiateAuthenticationResult.Error is InitiateAuthenticationError.Challenge value
            && value.ChallengeType == UkpsChallengeType.MultiFactorAuthenticationSetupRequired
                ? value
                : throw new InvalidOperationException("Unexpected challenge");

        var result = await _webIdentityAdministrationService.AssociateSoftwareToken(
            challenge.AuthenticationSessionId,
            cancellationToken
        );

        string issuer = "UKPS";
        return new Uri(
            $"otpauth://totp/{Uri.EscapeDataString($"{issuer}:{userEmail}")}"
                + $"?secret={Uri.EscapeDataString(result.Secret)}"
                + $"&issuer={Uri.EscapeDataString(issuer)}"
                + $"&algorithm=SHA1"
                + $"&digits=6"
                + $"&period=30"
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
