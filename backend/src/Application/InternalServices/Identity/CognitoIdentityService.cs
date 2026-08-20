using System.Security.Cryptography;
using System.Text;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Options;
using UKPS.Api.Application.Authentication;
using UKPS.Api.Application.Authentication.Dtos;
using UKPS.Api.Application.Authentication.Errors;
using CreateNewUserResult = UKPS.Api.Application.Common.Result<
    string,
    UKPS.Api.Application.InternalServices.Identity.CreateNewUserError
>;
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
    private readonly IOptions<CognitoConfiguration> _options;
    private readonly ILogger<CognitoIdentityService> _logger;

    public CognitoIdentityService(
        IAmazonCognitoIdentityProvider cognito,
        IOptions<CognitoConfiguration> options,
        ILogger<CognitoIdentityService> logger
    )
    {
        _cognito = cognito;
        _options = options;
        _logger = logger;
    }

    public async Task<CreateNewUserResult> CreateNewUser(
        string email,
        CancellationToken cancellationToken
    )
    {
        var request = new AdminCreateUserRequest
        {
            UserPoolId = _options.Value.UserPoolId,
            Username = email,
            UserAttributes = [new() { Name = "email", Value = email }],
            MessageAction = "SUPPRESS",
        };

        try
        {
            var response = await _cognito.AdminCreateUserAsync(request, cancellationToken);
            var sub =
                response
                    .User.Attributes.FirstOrDefault(x =>
                        string.Equals(x.Name, "sub", StringComparison.Ordinal)
                    )
                    ?.Value
                ?? throw new InvalidOperationException(
                    "Cognito created the user but did not return the expected 'sub' attribute."
                );
            return CreateNewUserResult.Ok(sub);
        }
        catch (UsernameExistsException)
        {
            return CreateNewUserResult.Err(new CreateNewUserError.UsernameAlreadyExists());
        }
    }

    public async Task<UpdatePasswordResult> UpdatePassword(
        string userEmail,
        string newPassword,
        CancellationToken cancellationToken
    )
    {
        var request = new AdminSetUserPasswordRequest
        {
            UserPoolId = _options.Value.UserPoolId,
            Username = userEmail,
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
        string userEmail,
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
                        ["USERNAME"] = userEmail,
                        ["PASSWORD"] = newPassword,
                        ["SECRET_HASH"] = GenerateSecretHash(
                            userEmail,
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
        catch (NotAuthorizedException ex)
        {
            LogCognitoTokenRevocationIgnored(ex.Message, ex);
        }
        catch (InvalidParameterException ex)
        {
            LogCognitoTokenRevocationIgnored(ex.Message, ex);
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
        string username,
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
                        ["USERNAME"] = username,
                        ["SECRET_HASH"] = GenerateSecretHash(
                            username,
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

    public async Task MarkEmailAsVerified(string username, CancellationToken cancellationToken)
    {
        await _cognito.AdminUpdateUserAttributesAsync(
            new AdminUpdateUserAttributesRequest
            {
                UserPoolId = _options.Value.UserPoolId,
                Username = username,
                UserAttributes = [new AttributeType { Name = "email_verified", Value = "true" }],
            },
            cancellationToken
        );
    }

    public async Task<InitiatedAuthenticationResult> RespondToMultiFactorAuthenticationChallenge(
        string username,
        string authenticationSession,
        string code,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return await SendCognitoAuthRequest(async () =>
            {
                var cognitoResponse = await _cognito.AdminRespondToAuthChallengeAsync(
                    new AdminRespondToAuthChallengeRequest
                    {
                        UserPoolId = _options.Value.UserPoolId,
                        ClientId = _options.Value.ClientId,
                        ChallengeName = ChallengeNameType.SOFTWARE_TOKEN_MFA,
                        Session = authenticationSession,
                        ChallengeResponses = new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["USERNAME"] = username,
                            ["SOFTWARE_TOKEN_MFA_CODE"] = code,
                            ["SECRET_HASH"] = GenerateSecretHash(
                                username,
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
        catch (CodeMismatchException)
        {
            return InitiatedAuthenticationResult.Err(
                new InitiateAuthenticationError.Unauthorised()
            );
        }
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
        };
        return lookup.TryGetValue(challengeName, out InitiateAuthenticationError? err)
            ? err
            : throw new NotSupportedException($"Unhandled challenge type [{challengeName}].");
    }

    private static string GenerateSecretHash(string username, string clientId, string clientSecret)
    {
        var key = Encoding.UTF8.GetBytes(clientSecret);
        var message = Encoding.UTF8.GetBytes(username + clientId);

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
        Level = LogLevel.Information,
        Message = "Cognito token revocation ignored: {Message}"
    )]
    private partial void LogCognitoTokenRevocationIgnored(string message, Exception exception);

    private record AuthResponse
    {
        public required ChallengeNameType? ChallengeName { get; init; }
        public required string? Session { get; init; }
        public required AuthenticationResultType? AuthenticationResult { get; init; }
    }
}
