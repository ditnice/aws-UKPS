using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.EntityFrameworkCore;
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
using UKPS.Api.Persistence.Enums;
using InitiateAuthenticationResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.AuthenticationCredentialsDto,
    UKPS.Api.Application.InternalServices.Identity.InitiateAuthenticationError
>;
using ResendSetupTokenResult = UKPS.Api.Application.Common.Result<
    System.Guid,
    UKPS.Api.Application.Authentication.Errors.ResendSetupTokenError
>;
using SetupSmsMfaResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.SmsMultiFactorAuthenticationSetupDto,
    UKPS.Api.Application.Authentication.Errors.SetupSmsMfaError
>;
using SetupTokenValidationResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.Authentication.Errors.SetupTokenValidationError>;
using SetupUserResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.MultiFactorAuthenticationSetupDto,
    UKPS.Api.Application.Authentication.Errors.UserSetupError
>;
using VerifyMultiFactorAuthenticationResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.AuthenticationCredentialsDto,
    UKPS.Api.Application.Authentication.Errors.VerifyMultiFactorAuthenticationError
>;
using VerifySmsMultiFactorAuthenticationResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.AuthenticationCredentialsDto,
    UKPS.Api.Application.Authentication.Errors.VerifyMultiFactorAuthenticationError
>;

namespace UKPS.Api.Application.Authentication;

internal partial class IdentityAdministrationService : IIdentityAdministrationService
{
    private readonly AppDbContext _appDbContext;
    private readonly IIdentityService _identityService;
    private readonly IOptions<UserOnboardingOptions> _options;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ISetupLinkCreator _setupLinkCreator;
    private readonly IEmailService _emailService;
    private readonly EmailOptions _configuration;

    public IdentityAdministrationService(
        AppDbContext appDbContext,
        IIdentityService identityService,
        IOptions<UserOnboardingOptions> options,
        IDateTimeProvider dateTimeProvider,
        ISetupLinkCreator setupLinkCreator,
        IEmailService emailService,
        IOptions<EmailOptions> configuration
    )
    {
        _appDbContext = appDbContext;
        _identityService = identityService;
        _options = options;
        _dateTimeProvider = dateTimeProvider;
        _setupLinkCreator = setupLinkCreator;
        _emailService = emailService;
        _configuration = configuration.Value;
    }

    public async Task<SetupUserResult> SetupUser(
        SetupUserCommand command,
        CancellationToken cancellationToken
    )
    {
        UserOnboardingRecord? userRecord = await GetUserOnboardingRecordForVerificationAsync(
            command.SetupToken,
            cancellationToken
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

        DateTime now = _dateTimeProvider.GetUtcNow();
        await PersistConsumedSetupAsync(userRecord, now, command.NewPassword, cancellationToken);

        return await CompletePasswordSetupAsync(userRecord, command.NewPassword, cancellationToken);
    }

    public async Task<VerifyMultiFactorAuthenticationResult> VerifyMultiFactorAuthentication(
        VerifyMultiFactorAuthenticationCommand command,
        CancellationToken cancellationToken
    )
    {
        UserOnboardingRecord? userRecord = await GetUserOnboardingRecordForVerificationAsync(
            command.SetupToken,
            cancellationToken
        );

        if (userRecord is null)
        {
            return VerifyMultiFactorAuthenticationResult.Err(
                new VerifyMultiFactorAuthenticationError.DoesNotExist()
            );
        }

        try
        {
            AuthenticationCredentialsDto credentials = await _identityService.VerifySoftwareToken(
                userRecord.ResultingUser!.CognitoUsername,
                command.AuthenticationSession,
                command.Code,
                cancellationToken
            );
            await _identityService.MarkEmailAsVerified(
                userRecord.ResultingUser.CognitoUsername,
                cancellationToken
            );
            userRecord.ResultingUser.FinaliseSetup();
            await _appDbContext.SaveChangesAsync(cancellationToken);
            return VerifyMultiFactorAuthenticationResult.Ok(credentials);
        }
        catch (EnableSoftwareTokenMFAException)
        {
            return VerifyMultiFactorAuthenticationResult.Err(
                new VerifyMultiFactorAuthenticationError.InvalidCode()
            );
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

    public async Task<ResendSetupTokenResult> ResendSetupToken(
        ResendSetupTokenCommand command,
        CancellationToken cancellationToken
    )
    {
        bool hasSetupToken = command.SetupToken is not null;
        bool hasCorrelationId = command.CorrelationId is not null;

        if (hasSetupToken == hasCorrelationId)
        {
            return ResendSetupTokenResult.Err(new ResendSetupTokenError.InvalidTokenCombination());
        }

        IQueryable<UserOnboardingRecord> onboardingRecords =
            _appDbContext.UserOnboardingRecords.Include(x => x.ResultingUser);
        UserOnboardingRecord? userRecord = hasCorrelationId
            ? await onboardingRecords.FirstOrDefaultAsync(
                x => x.CorrelationId == command.CorrelationId,
                cancellationToken
            )
            : await onboardingRecords.FirstOrDefaultAsync(
                x => x.SetupToken == command.SetupToken,
                cancellationToken
            );

        if (userRecord is null)
        {
            return ResendSetupTokenResult.Err(new ResendSetupTokenError.DoesNotExist());
        }

        if (userRecord.ConsumedAt is not null)
        {
            return ResendSetupTokenResult.Err(new ResendSetupTokenError.Consumed());
        }

        if (userRecord.ResendCount >= _configuration.MaxResendSignUpLinkAttempts)
        {
            return ResendSetupTokenResult.Err(new ResendSetupTokenError.TooManyAttempts());
        }

        User user = userRecord.ResultingUser!;
        UserOnboardingRecord newRecord = await ReplaceOnboardingRecord(
            userRecord,
            cancellationToken
        );
        await SendSetupLinkEmail(user, newRecord.SetupToken, cancellationToken);

        return ResendSetupTokenResult.Ok(
            newRecord.CorrelationId
                ?? throw new UnreachableException(
                    "ReplaceOnboardingRecord always assigns a CorrelationId to the new record."
                )
        );
    }

    private async Task<UserOnboardingRecord> ReplaceOnboardingRecord(
        UserOnboardingRecord existingRecord,
        CancellationToken cancellationToken
    )
    {
        var newRecord = new UserOnboardingRecord()
        {
            OnboardingFlow = existingRecord.OnboardingFlow,
            SetupToken = Guid.CreateVersion7(),
            CorrelationId = Guid.CreateVersion7(),
            CreatedByUserId = existingRecord.CreatedByUserId,
            CreatedAt = _dateTimeProvider.GetUtcNow(),
            ResultingUserId = existingRecord.ResultingUserId,
            ResendCount = existingRecord.ResendCount + 1,
        };

        _appDbContext.UserOnboardingRecords.Remove(existingRecord);
        _appDbContext.UserOnboardingRecords.Add(newRecord);
        await _appDbContext.SaveChangesAsync(cancellationToken);

        return newRecord;
    }

    private async Task SendSetupLinkEmail(
        User user,
        Guid setupToken,
        CancellationToken cancellationToken
    )
    {
        OnboardingFlow onboardingFlow =
            user.OnboardingRecord?.OnboardingFlow
            ?? throw new InvalidOperationException(
                "User's onboarding record was not set as expected."
            );
        Uri link = _setupLinkCreator.GetSetupLink(setupToken);
        IEmail email = onboardingFlow switch
        {
            OnboardingFlow.Invite => new UserSignUpRequestEmail() { Link = link },
            OnboardingFlow.SelfSignUp => new UserMembershipRequestApprovedNotificationEmail()
            {
                Link = link,
            },
            _ => throw new UnreachableException(
                $"Unexpected onboarding flow [{user.OnboardingRecord.OnboardingFlow}] encountered when sending setup link email."
            ),
        };
        await _emailService.SendEmail(
            new SendEmailCommand()
            {
                PersonIdentifier = user.CognitoUsername,
                RecipientAddress = user.WorkEmail,
                Email = email,
            },
            cancellationToken
        );
    }

    public async Task<SetupSmsMfaResult> SetupSmsMfa(
        SetupSmsMfaCommand command,
        CancellationToken cancellationToken
    )
    {
        UserOnboardingRecord? userRecord = await GetUserOnboardingRecordAsync(
            command.SetupToken,
            cancellationToken
        );

        if (userRecord is null)
        {
            return SetupSmsMfaResult.Err(new SetupSmsMfaError.DoesNotExist());
        }

        DateTime now = _dateTimeProvider.GetUtcNow();

        if (!IsWithinSetupWindow(userRecord, now))
        {
            return SetupSmsMfaResult.Err(new SetupSmsMfaError.Expired());
        }

        CognitoUsername cognitoUsername = userRecord.ResultingUser!.CognitoUsername;

        string? storedPassword = DecryptPassword(
            userRecord.EncryptedPassword,
            userRecord.PasswordExpiresAt,
            now,
            _options.Value.PasswordStorageKey
        );
        if (storedPassword is null)
        {
            return SetupSmsMfaResult.Err(new SetupSmsMfaError.PasswordUnavailable());
        }

        if (!PhoneNumberValidator.IsValidSmsNumber(command.PhoneNumber, out string? e164Number))
        {
            return SetupSmsMfaResult.Err(new SetupSmsMfaError.InvalidPhoneNumber());
        }

        SetupSmsMfaResult? attemptError = await ValidateAndRecordSmsSetupAttemptAsync(
            userRecord,
            now,
            cancellationToken
        );
        if (attemptError is not null)
        {
            return attemptError.Value;
        }

        return await EnableSmsMfaAndRestartAsync(
            cognitoUsername,
            e164Number!,
            storedPassword,
            cancellationToken
        );
    }

    public async Task<VerifySmsMultiFactorAuthenticationResult> VerifySmsMultiFactorAuthentication(
        VerifySmsMultiFactorAuthenticationCommand command,
        CancellationToken cancellationToken
    )
    {
        UserOnboardingRecord? userRecord = await GetUserOnboardingRecordForVerificationAsync(
            command.SetupToken,
            cancellationToken
        );

        if (userRecord is null || userRecord.ResultingUser is null)
        {
            return VerifySmsMultiFactorAuthenticationResult.Err(
                new VerifyMultiFactorAuthenticationError.DoesNotExist()
            );
        }

        if (userRecord.ResultingUser.IsSetupComplete)
        {
            return VerifySmsMultiFactorAuthenticationResult.Err(
                new VerifyMultiFactorAuthenticationError.AlreadyCompleted()
            );
        }

        if (!IsWithinSetupWindow(userRecord, _dateTimeProvider.GetUtcNow()))
        {
            return VerifySmsMultiFactorAuthenticationResult.Err(
                new VerifyMultiFactorAuthenticationError.Expired()
            );
        }

        CognitoUsername cognitoUsername = userRecord.ResultingUser.CognitoUsername;

        return await VerifySmsCodeAndFinaliseAsync(
            userRecord,
            cognitoUsername,
            command,
            cancellationToken
        );
    }

#pragma warning disable CA1031
    private async Task<SetupSmsMfaResult> EnableSmsMfaAndRestartAsync(
        CognitoUsername cognitoUsername,
        string e164Number,
        string storedPassword,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await _identityService.SetUserPhoneNumber(
                cognitoUsername,
                e164Number,
                cancellationToken
            );
            await _identityService.SetUserMfaPreference(
                cognitoUsername,
                smsEnabled: true,
                smsPreferred: true,
                cancellationToken,
                softwareTokenEnabled: false
            );

            SetupSmsMfaResult restartResult = await RestartAuthenticationForSmsMfaAsync(
                cognitoUsername,
                storedPassword,
                cancellationToken
            );

            if (restartResult.IsErr)
            {
                await RevertSmsMfaPreferenceAsync(cognitoUsername);
            }

            return restartResult;
        }
        catch
        {
            await RevertSmsMfaPreferenceAsync(cognitoUsername);
            throw;
        }
    }

    private async Task RevertSmsMfaPreferenceAsync(CognitoUsername cognitoUsername)
    {
        try
        {
            await _identityService.SetUserMfaPreference(
                cognitoUsername,
                smsEnabled: false,
                smsPreferred: false,
                CancellationToken.None,
                softwareTokenEnabled: false
            );
        }
        catch (Exception)
        {
            GC.KeepAlive(null);
        }
    }
#pragma warning restore CA1031

    private async Task<SetupSmsMfaResult> RestartAuthenticationForSmsMfaAsync(
        CognitoUsername cognitoUsername,
        string storedPassword,
        CancellationToken cancellationToken
    )
    {
        InitiateAuthenticationResult restartResult = await _identityService.InitiateAuthentication(
            cognitoUsername,
            storedPassword,
            cancellationToken
        );

        return restartResult.Match(
            _ => SetupSmsMfaResult.Err(new SetupSmsMfaError.Unauthorised()),
            err =>
                err.Match(
                    unauthorised: () => SetupSmsMfaResult.Err(new SetupSmsMfaError.Unauthorised()),
                    challenge: challenge =>
                        challenge.ChallengeType
                        == UkpsChallengeType.MultiFactorAuthenticationRequired
                            ? SetupSmsMfaResult.Ok(
                                new SmsMultiFactorAuthenticationSetupDto
                                {
                                    AuthenticationSession = challenge.AuthenticationSession,
                                }
                            )
                            : SetupSmsMfaResult.Err(new SetupSmsMfaError.Unauthorised())
                )
        );
    }

    private bool IsWithinSetupWindow(UserOnboardingRecord record, DateTime now)
    {
        TimeSpan window = TimeSpan.FromSeconds(_options.Value.SetupTokenExpiryTimeSeconds);
        return record.CreatedAt.Add(window) >= now;
    }

    private async Task<UserOnboardingRecord?> GetUserOnboardingRecordAsync(
        Guid setupToken,
        CancellationToken cancellationToken
    )
    {
        return await _appDbContext
            .UserOnboardingRecords.Include(x => x.ResultingUser)
            .FirstOrDefaultAsync(
                x => x.SetupToken == setupToken,
                cancellationToken: cancellationToken
            );
    }

    private async Task<UserOnboardingRecord?> GetUserOnboardingRecordForVerificationAsync(
        Guid setupToken,
        CancellationToken cancellationToken
    )
    {
        return await _appDbContext
            .UserOnboardingRecords.Include(x => x.ResultingUser)
                .ThenInclude(x => x!.UserOrgMemberships)
            .FirstOrDefaultAsync(
                x => x.SetupToken == setupToken,
                cancellationToken: cancellationToken
            );
    }

    private async Task PersistConsumedSetupAsync(
        UserOnboardingRecord userRecord,
        DateTime now,
        string newPassword,
        CancellationToken cancellationToken
    )
    {
        userRecord.MarkAsConsumed(now);
        userRecord.StorePassword(
            EncryptPassword(newPassword, _options.Value.PasswordStorageKey),
            now.AddSeconds(_options.Value.StoredPasswordExpirySeconds)
        );
        await _appDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<SetupUserResult> CompletePasswordSetupAsync(
        UserOnboardingRecord userRecord,
        string newPassword,
        CancellationToken cancellationToken
    )
    {
        Result<UpdatePasswordError> updatePasswordResult = await _identityService.UpdatePassword(
            userRecord.ResultingUser!.CognitoUsername,
            newPassword,
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
            userRecord.ResultingUser.CognitoUsername,
            userRecord.ResultingUser.WorkEmail,
            newPassword,
            cancellationToken
        );
    }

    private async Task<SetupSmsMfaResult?> ValidateAndRecordSmsSetupAttemptAsync(
        UserOnboardingRecord userRecord,
        DateTime now,
        CancellationToken cancellationToken
    )
    {
        if (userRecord.SmsSetupAttemptCount >= _options.Value.MaxSmsSetupAttempts)
        {
            return SetupSmsMfaResult.Err(new SetupSmsMfaError.TooManyAttempts());
        }

        if (
            userRecord.LastSmsSetupAttemptAt is { } lastAttempt
            && lastAttempt.AddSeconds(_options.Value.SmsSetupCooldownSeconds) > now
        )
        {
            return SetupSmsMfaResult.Err(new SetupSmsMfaError.TooSoon());
        }

        userRecord.RecordSmsSetupAttempt(now);
        try
        {
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return SetupSmsMfaResult.Err(new SetupSmsMfaError.ConcurrentRequest());
        }

        return null;
    }

#pragma warning disable CA1031
    private async Task<VerifySmsMultiFactorAuthenticationResult> VerifySmsCodeAndFinaliseAsync(
        UserOnboardingRecord userRecord,
        CognitoUsername cognitoUsername,
        VerifySmsMultiFactorAuthenticationCommand command,
        CancellationToken cancellationToken
    )
    {
        AuthenticationCredentialsDto credentials;
        try
        {
            credentials = await _identityService.RespondToSmsMultiFactorAuthenticationChallenge(
                cognitoUsername,
                command.AuthenticationSession,
                command.Code,
                cancellationToken
            );
        }
        catch (CodeMismatchException)
        {
            return VerifySmsMultiFactorAuthenticationResult.Err(
                new VerifyMultiFactorAuthenticationError.InvalidCode()
            );
        }
        catch (Exception ex) when (ex is NotAuthorizedException or ExpiredCodeException)
        {
            await RevertSmsMfaPreferenceAsync(cognitoUsername);

            return VerifySmsMultiFactorAuthenticationResult.Err(
                new VerifyMultiFactorAuthenticationError.SessionExpired()
            );
        }
        catch (Exception ex)
            when (ex
                    is TooManyRequestsException
                        or LimitExceededException
                        or TooManyFailedAttemptsException
            )
        {
            return VerifySmsMultiFactorAuthenticationResult.Err(
                new VerifyMultiFactorAuthenticationError.RateLimited()
            );
        }

        await _identityService.MarkEmailAsVerified(cognitoUsername, cancellationToken);

        userRecord.ClearStoredPassword();
        userRecord.ResultingUser!.FinaliseSetup();
        await _appDbContext.SaveChangesAsync(cancellationToken);

        return VerifySmsMultiFactorAuthenticationResult.Ok(credentials);
    }

    private static string EncryptPassword(string password, string key)
    {
        using var aes = Aes.Create();
        aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        byte[] plainBytes = Encoding.UTF8.GetBytes(password);
        byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        byte[] result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);
        return Convert.ToBase64String(result);
    }

    private static string? DecryptPassword(
        string? encryptedPassword,
        DateTime? passwordExpiresAt,
        DateTime now,
        string key
    )
    {
        if (
            string.IsNullOrWhiteSpace(encryptedPassword)
            || passwordExpiresAt is null
            || passwordExpiresAt < now
        )
        {
            return null;
        }

        try
        {
            byte[] data = Convert.FromBase64String(encryptedPassword);
            if (data.Length < 16)
            {
                return null;
            }

            using var aes = Aes.Create();
            aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes(key));

            byte[] iv = new byte[16];
            Buffer.BlockCopy(data, 0, iv, 0, iv.Length);
            aes.IV = iv;

            byte[] cipherBytes = new byte[data.Length - iv.Length];
            Buffer.BlockCopy(data, iv.Length, cipherBytes, 0, cipherBytes.Length);

            using var decryptor = aes.CreateDecryptor();
            byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }
        catch (CryptographicException)
        {
            return null;
        }
        catch (FormatException)
        {
            return null;
        }
    }
#pragma warning restore CA1031
}
