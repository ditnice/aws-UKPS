using System.Diagnostics;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using UKPS.Api.Application.Authentication.Dtos;
using UKPS.Api.Application.Authentication.Errors;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.InternalServices.Communication;
using UKPS.Api.Application.InternalServices.Hosting;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Application.InternalServices.Temporal;
using UKPS.Api.Application.Users;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.Identity;
using InitiateAuthenticationResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.AuthenticationCredentialsDto,
    UKPS.Api.Application.InternalServices.Identity.InitiateAuthenticationError
>;
using ResendSetupTokenResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.Authentication.Errors.ResendSetupTokenError>;
using SetupTokenValidationResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.Authentication.Errors.SetupTokenValidationError>;
using SetupUserResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.MultiFactorAuthenticationSetupDto,
    UKPS.Api.Application.Authentication.Errors.UserSetupError
>;
using VerifyMultiFactorAuthenticationResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.AuthenticationCredentialsDto,
    UKPS.Api.Application.Authentication.Errors.VerifyMultiFactorAuthenticationError
>;

namespace UKPS.Api.Application.Authentication;

internal class IdentityAdministrationService : IIdentityAdministrationService
{
    private readonly AppDbContext _appDbContext;
    private readonly IIdentityService _identityService;
    private readonly IOptions<UserOnboardingOptions> _options;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ISetupLinkCreator _setupLinkCreator;
    private readonly IEmailService _emailService;

    public IdentityAdministrationService(
        AppDbContext appDbContext,
        IIdentityService identityService,
        IOptions<UserOnboardingOptions> options,
        IDateTimeProvider dateTimeProvider,
        ISetupLinkCreator setupLinkCreator,
        IEmailService emailService
    )
    {
        _appDbContext = appDbContext;
        _identityService = identityService;
        _options = options;
        _dateTimeProvider = dateTimeProvider;
        _setupLinkCreator = setupLinkCreator;
        _emailService = emailService;
    }

    public async Task<SetupUserResult> SetupUser(
        SetupUserCommand command,
        CancellationToken cancellationToken
    )
    {
        UserOnboardingRecord? userRecord = await _appDbContext
            .UserOnboardingRecords.Include(x => x.User)
                .ThenInclude(x => x!.UserOrgMemberships)
            .FirstOrDefaultAsync(
                x => x.SetupToken == command.SetupToken,
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
            userRecord.User!.CognitoUsername,
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
            userRecord.User.CognitoUsername,
            userRecord.User.WorkEmail,
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
            await _appDbContext
                .UserOnboardingRecords.Include(x => x.User)
                    .ThenInclude(x => x!.UserOrgMemberships)
                .FirstOrDefaultAsync(
                    x => x.SetupToken == command.SetupToken,
                    cancellationToken: cancellationToken
                )
            ?? throw new InvalidOperationException();

        try
        {
            AuthenticationCredentialsDto credentials = await _identityService.VerifySoftwareToken(
                userRecord.User!.CognitoUsername,
                command.AuthenticationSession,
                command.Code,
                cancellationToken
            );
            await _identityService.MarkEmailAsVerified(
                userRecord.User.CognitoUsername,
                cancellationToken
            );
            userRecord.User.FinaliseSetup();
            await _appDbContext.SaveChangesAsync(cancellationToken);
            return VerifyMultiFactorAuthenticationResult.Ok(credentials);
        }
        catch (CodeMismatchException)
        {
            return VerifyMultiFactorAuthenticationResult.Err(
                new VerifyMultiFactorAuthenticationError.InvalidCode()
            );
        }
    }

    private async Task<SetupUserResult> InitiateAuthenticationAndGetOtp(
        CognitoUsername userIdentityId,
        string userEmail,
        string newPassword,
        CancellationToken cancellationToken
    )
    {
        InitiateAuthenticationResult initiateAuthenticationResult =
            await _identityService.InitiateAuthentication(
                userIdentityId,
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
            challenge.AuthenticationSession,
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
        UserOnboardingRecord? userRecord =
            await _appDbContext.UserOnboardingRecords.FirstOrDefaultAsync(
                x => x.SetupToken == setupToken,
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

    private const int MaxResendAttempts = 3;

    public async Task<ResendSetupTokenResult> ResendSetupToken(
        ResendSetupTokenCommand command,
        CancellationToken cancellationToken
    )
    {
        UserOnboardingRecord? userRecord = await _appDbContext
            .UserOnboardingRecords.Include(x => x.User)
            .FirstOrDefaultAsync(
                x => x.SetupToken == command.SetupToken,
                cancellationToken: cancellationToken
            );

        if (userRecord is null)
        {
            return ResendSetupTokenResult.Err(new ResendSetupTokenError.DoesNotExist());
        }

        if (userRecord.ConsumedAt is not null)
        {
            return ResendSetupTokenResult.Err(new ResendSetupTokenError.Consumed());
        }

        if (userRecord.ResendCount >= MaxResendAttempts)
        {
            return ResendSetupTokenResult.Err(new ResendSetupTokenError.TooManyAttempts());
        }

        UserOnboardingRecord newRecord = await ReplaceOnboardingRecord(
            userRecord,
            cancellationToken
        );
        await SendSetupLinkEmail(userRecord.User!, newRecord.SetupToken, cancellationToken);

        return ResendSetupTokenResult.Ok();
    }

    private async Task<UserOnboardingRecord> ReplaceOnboardingRecord(
        UserOnboardingRecord existingRecord,
        CancellationToken cancellationToken
    )
    {
        var newRecord = new UserOnboardingRecord()
        {
            SetupToken = Guid.CreateVersion7(),
            CreatedBy = existingRecord.CreatedBy,
            CreatedAt = _dateTimeProvider.GetUtcNow(),
            UserId = existingRecord.UserId,
            ResendCount = existingRecord.ResendCount + 1,
        };

        await using IDbContextTransaction transaction =
            await _appDbContext.Database.BeginTransactionAsync(cancellationToken);

        _appDbContext.UserOnboardingRecords.Remove(existingRecord);
        await _appDbContext.SaveChangesAsync(cancellationToken);

        _appDbContext.UserOnboardingRecords.Add(newRecord);
        await _appDbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return newRecord;
    }

    private async Task SendSetupLinkEmail(
        User user,
        Guid setupToken,
        CancellationToken cancellationToken
    )
    {
        Uri link = _setupLinkCreator.GetSetupLink(setupToken);
        await _emailService.SendEmail(
            new SendEmailCommand()
            {
                CognitoUsername = user.CognitoUsername,
                RecipientAddress = user.WorkEmail,
                Email = new UserSignUpRequestEmail() { Link = link },
            },
            cancellationToken
        );
    }
}
