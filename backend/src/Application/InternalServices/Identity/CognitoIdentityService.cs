using System.Security.Cryptography;
using System.Text;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Options;
using UKPS.Api.Application.Authentication;
using UKPS.Api.Application.Authentication.Dtos;
using UKPS.Api.Application.Authentication.Errors;
using UKPS.Api.Persistence.Entities.Identity;
using CreateNewUserResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.InternalServices.Identity.CreateNewUserError>;
using InitiatedAuthenticationResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.AuthenticationCredentialsDto,
    UKPS.Api.Application.InternalServices.Identity.InitiateAuthenticationError
>;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using UpdatePasswordResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.InternalServices.Identity.UpdatePasswordError>;

namespace UKPS.Api.Application.InternalServices.Identity;

internal sealed partial class CognitoIdentityService : IIdentityService
{
    private readonly IAmazonCognitoIdentityProvider _cognito;
    private readonly IOptions<CognitoOptions> _options;
    private readonly ILogger<CognitoIdentityService> _logger;

    public CognitoIdentityService(
        IAmazonCognitoIdentityProvider cognito,
        IOptions<CognitoOptions> options,
        ILogger<CognitoIdentityService> logger
    )
    {
        _cognito = cognito;
        _options = options;
        _logger = logger;
    }

    public async Task<CreateNewUserResult> CreateNewUser(
        CognitoUsername cognitoUsername,
        string email,
        CancellationToken cancellationToken
    )
    {
        var request = new AdminCreateUserRequest
        {
            UserPoolId = _options.Value.UserPoolId,
            Username = cognitoUsername.Value,
            UserAttributes = [new() { Name = "email", Value = email }],
            MessageAction = "SUPPRESS",
        };

        try
        {
            await _cognito.AdminCreateUserAsync(request, cancellationToken);
            return CreateNewUserResult.Ok();
        }
        catch (UsernameExistsException)
        {
            return CreateNewUserResult.Err(new CreateNewUserError.UsernameAlreadyExists());
        }
    }

    public async Task<UpdatePasswordResult> UpdatePassword(
        CognitoUsername cognitoUsername,
        string newPassword,
        CancellationToken cancellationToken
    )
    {
        var request = new AdminSetUserPasswordRequest
        {
            UserPoolId = _options.Value.UserPoolId,
            Username = cognitoUsername.Value,
            Password = newPassword,
            Permanent = true,
        };

        try
        {
            await _cognito.AdminSetUserPasswordAsync(request, cancellationToken);
        }
        catch (InvalidPasswordException)
        {
            return UpdatePasswordResult.Err(new UpdatePasswordError.InvalidPassword());
        }

        return UpdatePasswordResult.Ok();
    }

    public Task<InitiatedAuthenticationResult> InitiateAuthentication(
        CognitoUsername cognitoUsername,
        string newPassword,
        CancellationToken cancellationToken
    )
    {
        return SendCognitoAuthRequest(async () =>
        {
            AdminInitiateAuthResponse cognitoResponse = await _cognito.AdminInitiateAuthAsync(
                new AdminInitiateAuthRequest
                {
                    UserPoolId = _options.Value.UserPoolId,
                    ClientId = _options.Value.ClientId,
                    AuthFlow = AuthFlowType.ADMIN_USER_PASSWORD_AUTH,
                    AuthParameters = new Dictionary<string, string>(StringComparer.InvariantCulture)
                    {
                        ["USERNAME"] = cognitoUsername.Value,
                        ["PASSWORD"] = newPassword,
                        ["SECRET_HASH"] = GenerateSecretHash(
                            cognitoUsername,
                            _options.Value.ClientId,
                            _options.Value.ClientSecret
                        ),
                    },
                },
                cancellationToken
            );
            return cognitoResponse is null
                ? null
                : new()
                {
                    ChallengeName = cognitoResponse.ChallengeName,
                    Session = cognitoResponse.Session,
                    AuthenticationResult = cognitoResponse.AuthenticationResult,
                };
        });
    }

    public async Task UpdateUserEmail(
        CognitoUsername cognitoUsername,
        string updatedEmail,
        CancellationToken cancellationToken
    )
    {
        await _cognito.AdminUpdateUserAttributesAsync(
            new AdminUpdateUserAttributesRequest
            {
                UserPoolId = _options.Value.UserPoolId,
                Username = cognitoUsername.Value,
                UserAttributes = [new AttributeType { Name = "email", Value = updatedEmail }],
            },
            cancellationToken
        );
    }

    public async Task DeleteUser(
        CognitoUsername cognitoUsername,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await _cognito.AdminDeleteUserAsync(
                new AdminDeleteUserRequest
                {
                    UserPoolId = _options.Value.UserPoolId,
                    Username = cognitoUsername.Value,
                },
                cancellationToken
            );
        }
        catch (UserNotFoundException)
        {
            // Already gone (e.g. a retry after a partial failure), so treat as deleted.
            LogCognitoUserAlreadyDeleted(cognitoUsername.Value);
        }
    }

    public async Task<InitiatedAuthenticationResult> RefreshAuthenticationToken(
        string refreshToken,
        CancellationToken cancellationToken
    )
    {
        LogRefreshingCognitoToken(_options.Value.ClientId, refreshToken?.Length ?? 0);
        try
        {
            return await SendCognitoAuthRequest(async () =>
            {
                var cognitoResponse = await _cognito.GetTokensFromRefreshTokenAsync(
                    new GetTokensFromRefreshTokenRequest
                    {
                        ClientId = _options.Value.ClientId,
                        ClientSecret = _options.Value.ClientSecret,
                        RefreshToken = refreshToken,
                    },
                    cancellationToken
                );
                return cognitoResponse is null
                    ? null
                    : new()
                    {
                        ChallengeName = null,
                        Session = null,
                        AuthenticationResult = cognitoResponse.AuthenticationResult,
                    };
            });
        }
        catch (RefreshTokenReuseException)
        {
            return InitiatedAuthenticationResult.Err(
                new InitiateAuthenticationError.Unauthorised()
            );
        }
    }

    public async Task RevokeRefreshToken(string refreshToken, CancellationToken cancellationToken)
    {
        try
        {
            await _cognito.RevokeTokenAsync(
                new RevokeTokenRequest
                {
                    ClientId = _options.Value.ClientId,
                    ClientSecret = _options.Value.ClientSecret,
                    Token = refreshToken,
                },
                cancellationToken
            );
        }
        catch (Exception ex) when (ex is NotAuthorizedException or InvalidParameterException)
        {
            LogCognitoTokenRevocationIgnored(ex.Message, ex);
        }
        // Sign-out must still clear the local session when Cognito is unavailable or throttling.
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            LogCognitoTokenRevocationFailed(ex.Message, ex);
        }
    }

    public async Task<AssociateSoftwareTokenResult> AssociateSoftwareToken(
        string authenticationSessionId,
        CancellationToken cancellationToken
    )
    {
        var associateResponse = await _cognito.AssociateSoftwareTokenAsync(
            new AssociateSoftwareTokenRequest { Session = authenticationSessionId },
            cancellationToken
        );

        return new()
        {
            Secret = associateResponse.SecretCode,
            AuthenticationSession = associateResponse.Session,
        };
    }

    public async Task<AuthenticationCredentialsDto> VerifySoftwareToken(
        CognitoUsername cognitoUsername,
        string authenticationSessionId,
        string code,
        CancellationToken cancellationToken
    )
    {
        VerifySoftwareTokenResponse verifyResponse = await _cognito.VerifySoftwareTokenAsync(
            new VerifySoftwareTokenRequest { Session = authenticationSessionId, UserCode = code },
            cancellationToken
        );
        AdminRespondToAuthChallengeResponse challengeResponse =
            await _cognito.AdminRespondToAuthChallengeAsync(
                new AdminRespondToAuthChallengeRequest
                {
                    UserPoolId = _options.Value.UserPoolId,
                    ClientId = _options.Value.ClientId,
                    ChallengeName = ChallengeNameType.MFA_SETUP,
                    Session = verifyResponse.Session,
                    ChallengeResponses = new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["USERNAME"] = cognitoUsername.Value,
                        ["SECRET_HASH"] = GenerateSecretHash(
                            cognitoUsername,
                            _options.Value.ClientId,
                            _options.Value.ClientSecret
                        ),
                        ["SOFTWARE_TOKEN_MFA_CODE"] = code,
                    },
                },
                cancellationToken
            );

        AuthenticationResultType auth =
            challengeResponse.AuthenticationResult
            ?? throw new NotAuthorizedException(
                "Cognito MFA setup did not return authentication credentials."
            );

        return new AuthenticationCredentialsDto
        {
            AccessToken = auth.AccessToken,
            RefreshToken = auth.RefreshToken,
        };
    }

    public async Task MarkEmailAsVerified(
        CognitoUsername cognitoUsername,
        CancellationToken cancellationToken
    )
    {
        await _cognito.AdminUpdateUserAttributesAsync(
            new AdminUpdateUserAttributesRequest
            {
                UserPoolId = _options.Value.UserPoolId,
                Username = cognitoUsername.Value,
                UserAttributes = [new AttributeType { Name = "email_verified", Value = "true" }],
            },
            cancellationToken
        );
    }

    public async Task<InitiatedAuthenticationResult> RespondToMultiFactorAuthenticationChallenge(
        CognitoUsername cognitoUsername,
        string authenticationSession,
        string code,
        CancellationToken cancellationToken
    )
    {
        UserMfaDetails? details = await GetUserMfaDetails(cognitoUsername, cancellationToken);
        bool useSms = details?.MfaMethod.Equals("Sms", StringComparison.Ordinal) ?? false;
        ChallengeNameType challengeName = useSms
            ? ChallengeNameType.SMS_MFA
            : ChallengeNameType.SOFTWARE_TOKEN_MFA;
        string codeKey = useSms ? "SMS_MFA_CODE" : "SOFTWARE_TOKEN_MFA_CODE";

        try
        {
            return await SendCognitoAuthRequest(async () =>
            {
                var cognitoResponse = await _cognito.AdminRespondToAuthChallengeAsync(
                    new AdminRespondToAuthChallengeRequest
                    {
                        UserPoolId = _options.Value.UserPoolId,
                        ClientId = _options.Value.ClientId,
                        ChallengeName = challengeName,
                        Session = authenticationSession,
                        ChallengeResponses = new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["USERNAME"] = cognitoUsername.Value,
                            [codeKey] = code,
                            ["SECRET_HASH"] = GenerateSecretHash(
                                cognitoUsername,
                                _options.Value.ClientId,
                                _options.Value.ClientSecret
                            ),
                        },
                    },
                    cancellationToken
                );
                return cognitoResponse is null
                    ? null
                    : new()
                    {
                        ChallengeName = cognitoResponse.ChallengeName,
                        Session = cognitoResponse.Session,
                        AuthenticationResult = cognitoResponse.AuthenticationResult,
                    };
            });
        }
        catch (EnableSoftwareTokenMFAException)
        {
            return InitiatedAuthenticationResult.Err(
                new InitiateAuthenticationError.Unauthorised()
            );
        }
        catch (CodeMismatchException)
        {
            return InitiatedAuthenticationResult.Err(
                new InitiateAuthenticationError.Unauthorised()
            );
        }
        catch (NotAuthorizedException)
        {
            return InitiatedAuthenticationResult.Err(
                new InitiateAuthenticationError.Unauthorised()
            );
        }
    }

    public async Task<AuthenticationCredentialsDto> RespondToSmsMultiFactorAuthenticationChallenge(
        CognitoUsername cognitoUsername,
        string authenticationSession,
        string code,
        CancellationToken cancellationToken
    )
    {
        try
        {
            AdminRespondToAuthChallengeResponse challengeResponse =
                await _cognito.AdminRespondToAuthChallengeAsync(
                    new AdminRespondToAuthChallengeRequest
                    {
                        UserPoolId = _options.Value.UserPoolId,
                        ClientId = _options.Value.ClientId,
                        ChallengeName = ChallengeNameType.SMS_MFA,
                        Session = authenticationSession,
                        ChallengeResponses = new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["USERNAME"] = cognitoUsername.Value,
                            ["SMS_MFA_CODE"] = code,
                            ["SECRET_HASH"] = GenerateSecretHash(
                                cognitoUsername,
                                _options.Value.ClientId,
                                _options.Value.ClientSecret
                            ),
                        },
                    },
                    cancellationToken
                );

            AuthenticationResultType auth =
                challengeResponse.AuthenticationResult
                ?? throw new NotAuthorizedException(
                    "Cognito SMS MFA challenge did not return authentication credentials."
                );

            return new AuthenticationCredentialsDto
            {
                AccessToken = auth.AccessToken,
                RefreshToken = auth.RefreshToken,
            };
        }
        catch (CodeMismatchException)
        {
            throw new CodeMismatchException("Cognito SMS MFA code did not match.");
        }
        catch (NotAuthorizedException)
        {
            throw;
        }
    }

    public async Task<bool> SendForgottenPasswordCommand(
        CognitoUsername username,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await _cognito.ForgotPasswordAsync(
                new ForgotPasswordRequest()
                {
                    ClientId = _options.Value.ClientId,
                    Username = username.Value,
                    SecretHash = GenerateSecretHash(
                        username,
                        _options.Value.ClientId,
                        _options.Value.ClientSecret
                    ),
                },
                cancellationToken
            );
        }
        catch (LimitExceededException)
        {
            return false;
        }
        catch (TooManyRequestsException)
        {
            return false;
        }
        return true;
    }

    public async Task<ConfirmNewPasswordResult> ConfirmNewPassword(
        CognitoUsername username,
        string confirmationCode,
        string newPassword,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await _cognito.ConfirmForgotPasswordAsync(
                new ConfirmForgotPasswordRequest()
                {
                    ClientId = _options.Value.ClientId,
                    Username = username.Value,
                    ConfirmationCode = confirmationCode,
                    Password = newPassword,
                    SecretHash = GenerateSecretHash(
                        username,
                        _options.Value.ClientId,
                        _options.Value.ClientSecret
                    ),
                },
                cancellationToken
            );
        }
        catch (CodeMismatchException)
        {
            return ConfirmNewPasswordResult.Err(new ConfirmNewPasswordError.CodeMismatch());
        }
        catch (InvalidPasswordException)
        {
            return ConfirmNewPasswordResult.Err(new ConfirmNewPasswordError.InvalidPassword());
        }
        catch (LimitExceededException)
        {
            return ConfirmNewPasswordResult.Err(new ConfirmNewPasswordError.TooManyRequests());
        }
        catch (TooManyRequestsException)
        {
            return ConfirmNewPasswordResult.Err(new ConfirmNewPasswordError.TooManyRequests());
        }
        catch (TooManyFailedAttemptsException)
        {
            return ConfirmNewPasswordResult.Err(
                new ConfirmNewPasswordError.TooManyFailedAttempts()
            );
        }
        catch (ExpiredCodeException)
        {
            return ConfirmNewPasswordResult.Err(new ConfirmNewPasswordError.CodeExpired());
        }
        catch (PasswordHistoryPolicyViolationException)
        {
            return ConfirmNewPasswordResult.Err(
                new ConfirmNewPasswordError.HistoryPolicyViolation()
            );
        }
        return ConfirmNewPasswordResult.Ok();
    }

    public async Task SetUserPhoneNumber(
        CognitoUsername cognitoUsername,
        string phoneNumber,
        CancellationToken cancellationToken
    )
    {
        await _cognito.AdminUpdateUserAttributesAsync(
            new AdminUpdateUserAttributesRequest
            {
                UserPoolId = _options.Value.UserPoolId,
                Username = cognitoUsername.Value,
                UserAttributes =
                [
                    new AttributeType { Name = "phone_number", Value = phoneNumber },
                    new AttributeType { Name = "phone_number_verified", Value = "true" },
                ],
            },
            cancellationToken
        );
    }

    public async Task<UserMfaDetails?> GetUserMfaDetails(
        CognitoUsername cognitoUsername,
        CancellationToken cancellationToken
    )
    {
        var response = await _cognito.AdminGetUserAsync(
            new AdminGetUserRequest
            {
                UserPoolId = _options.Value.UserPoolId,
                Username = cognitoUsername.Value,
            },
            cancellationToken
        );

        var enabledMfas = response.UserMFASettingList ?? [];
        if (enabledMfas.Count == 0)
        {
            return null;
        }

        bool smsEnabled = enabledMfas.Contains("SMS_MFA", StringComparer.Ordinal);
        bool totpPreferred = string.Equals(
            response.PreferredMfaSetting,
            "SOFTWARE_TOKEN_MFA",
            StringComparison.Ordinal
        );
        string method = smsEnabled && !totpPreferred ? "Sms" : "AuthenticatorApp";

        string phoneNumber = string.Empty;
        if (method.Equals("Sms", StringComparison.Ordinal))
        {
            var phoneNumberAttribute = response.UserAttributes.FirstOrDefault(attr =>
                attr.Name.Equals("phone_number", StringComparison.Ordinal)
            );
            phoneNumber = ObfuscatePhoneNumber(phoneNumberAttribute?.Value ?? string.Empty);
        }

        return new UserMfaDetails { MfaMethod = method, MfaPhoneNumber = phoneNumber };
    }

    public async Task SetUserMfaPreference(
        CognitoUsername cognitoUsername,
        bool smsEnabled,
        bool smsPreferred,
        CancellationToken cancellationToken,
        bool softwareTokenEnabled = true
    )
    {
        await _cognito.AdminSetUserMFAPreferenceAsync(
            new AdminSetUserMFAPreferenceRequest
            {
                UserPoolId = _options.Value.UserPoolId,
                Username = cognitoUsername.Value,
                SMSMfaSettings = new SMSMfaSettingsType
                {
                    Enabled = smsEnabled,
                    PreferredMfa = smsPreferred,
                },
                SoftwareTokenMfaSettings = new SoftwareTokenMfaSettingsType
                {
                    Enabled = softwareTokenEnabled,
                    PreferredMfa = softwareTokenEnabled && !smsPreferred,
                },
            },
            cancellationToken
        );
    }

    private static string ObfuscatePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length < 4)
        {
            return string.Empty;
        }

        return phoneNumber[..2] + "**" + " " + "***" + phoneNumber[^2..];
    }

    private async Task<InitiatedAuthenticationResult> SendCognitoAuthRequest(
        Func<Task<AuthResponse?>> func
    )
    {
        try
        {
            var cognitoResponse = await func();

            if (cognitoResponse?.ChallengeName is not null)
            {
                var session =
                    cognitoResponse.Session
                    ?? throw new InvalidOperationException(
                        "Cognito response challenge has no session."
                    );
                return InitiatedAuthenticationResult.Err(
                    ConvertChallengeToError(cognitoResponse.ChallengeName, session)
                );
            }

            var auth = cognitoResponse?.AuthenticationResult;

            if (auth is null)
            {
                return InitiatedAuthenticationResult.Err(
                    new InitiateAuthenticationError.Unauthorised()
                );
            }

            var response = new AuthenticationCredentialsDto
            {
                AccessToken = auth.AccessToken,
                RefreshToken = auth.RefreshToken,
            };
            return InitiatedAuthenticationResult.Ok(response);
        }
        catch (NotAuthorizedException ex)
        {
            LogCognitoAuthenticationFailed(ex.Message, ex);
            return InitiatedAuthenticationResult.Err(
                new InitiateAuthenticationError.Unauthorised()
            );
        }
    }

    private static InitiateAuthenticationError ConvertChallengeToError(
        ChallengeNameType challengeName,
        string session
    )
    {
        var lookup = new Dictionary<ChallengeNameType, InitiateAuthenticationError>()
        {
            [ChallengeNameType.MFA_SETUP] = new InitiateAuthenticationError.Challenge(
                UkpsChallengeType.MultiFactorAuthenticationSetupRequired,
                session
            ),
            [ChallengeNameType.SOFTWARE_TOKEN_MFA] = new InitiateAuthenticationError.Challenge(
                UkpsChallengeType.MultiFactorAuthenticationRequired,
                session
            ),
            [ChallengeNameType.SMS_MFA] = new InitiateAuthenticationError.Challenge(
                UkpsChallengeType.MultiFactorAuthenticationRequired,
                session
            ),
        };
        return lookup.TryGetValue(challengeName, out InitiateAuthenticationError? err)
            ? err
            : throw new NotSupportedException($"Unhandled challenge type [{challengeName}].");
    }

    private static string GenerateSecretHash(
        CognitoUsername cognitoUsername,
        string clientId,
        string clientSecret
    )
    {
        var key = Encoding.UTF8.GetBytes(clientSecret);
        var message = Encoding.UTF8.GetBytes(cognitoUsername.Value + clientId);

        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(message);

        return Convert.ToBase64String(hash);
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Refreshing Cognito token. ClientId={ClientId}, RefreshTokenLength={RefreshTokenLength}"
    )]
    private partial void LogRefreshingCognitoToken(string clientId, int refreshTokenLength);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Cognito authentication failed: {Message}")]
    private partial void LogCognitoAuthenticationFailed(string message, Exception exception);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Cognito user {CognitoUsername} was not found when deleting; treating as already deleted."
    )]
    private partial void LogCognitoUserAlreadyDeleted(string cognitoUsername);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Cognito token revocation ignored: {Message}"
    )]
    private partial void LogCognitoTokenRevocationIgnored(string message, Exception exception);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Cognito token revocation failed: {Message}"
    )]
    private partial void LogCognitoTokenRevocationFailed(string message, Exception exception);

    private record AuthResponse
    {
        public required ChallengeNameType? ChallengeName { get; init; }
        public required string? Session { get; init; }
        public required AuthenticationResultType? AuthenticationResult { get; init; }
    }
}
