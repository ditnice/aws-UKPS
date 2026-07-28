using System.Security.Cryptography;
using System.Text;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Options;
using UKPS.Api.Application.Authentication.Dtos;
using UKPS.Api.Application.Authentication.Errors;
using LoginResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.AuthenticationCredentialsDto,
    UKPS.Api.Application.Authentication.Errors.LoginError
>;
using UpdatePasswordResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.AuthenticationCredentialsDto,
    UKPS.Api.Application.Authentication.Errors.UpdatePasswordError
>;

namespace UKPS.Api.Application.Authentication;

internal class AuthenticationService : IAuthenticationService
{
    private readonly IAmazonCognitoIdentityProvider _cognito;
    private readonly IOptions<CognitoConfiguration> _options;

    public AuthenticationService(
        IAmazonCognitoIdentityProvider cognito,
        IOptions<CognitoConfiguration> options
    )
    {
        _cognito = cognito;
        _options = options;
    }

    public async Task<LoginResult> Login(
        LoginRequest loginRequest,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(loginRequest);

        try
        {
            AdminInitiateAuthResponse cognitoResponse = await _cognito.AdminInitiateAuthAsync(
                new AdminInitiateAuthRequest
                {
                    UserPoolId = _options.Value.UserPoolId,
                    ClientId = _options.Value.ClientId,
                    AuthFlow = AuthFlowType.ADMIN_USER_PASSWORD_AUTH,
                    AuthParameters = new Dictionary<string, string>(StringComparer.InvariantCulture)
                    {
                        ["USERNAME"] = loginRequest.Username,
                        ["PASSWORD"] = loginRequest.Password,
                        ["SECRET_HASH"] = GenerateSecretHash(
                            loginRequest.Username,
                            _options.Value.ClientId,
                            _options.Value.ClientSecret
                        ),
                    },
                },
                cancellationToken
            );

            if (cognitoResponse?.ChallengeName is not null)
            {
                return LoginResult.Err(ConvertChallengeToError(cognitoResponse));
            }

            var auth = cognitoResponse?.AuthenticationResult;

            if (auth is null)
            {
                return LoginResult.Err(new LoginError.Unauthorised());
            }

            var response = new AuthenticationCredentialsDto { AccessToken = auth.AccessToken };
            return LoginResult.Ok(response);
        }
        catch (NotAuthorizedException)
        {
            return LoginResult.Err(new LoginError.Unauthorised());
        }
    }

    public async Task<UpdatePasswordResult> UpdatePassword(
        UpdatePasswordCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            AdminRespondToAuthChallengeResponse cognitoResponse =
                await _cognito.AdminRespondToAuthChallengeAsync(
                    new AdminRespondToAuthChallengeRequest
                    {
                        UserPoolId = _options.Value.UserPoolId,
                        ClientId = _options.Value.ClientId,
                        ChallengeName = ChallengeNameType.NEW_PASSWORD_REQUIRED,
                        Session = command.AuthenticationSessionId,
                        ChallengeResponses = new Dictionary<string, string>(
                            StringComparer.InvariantCulture
                        )
                        {
                            ["USERNAME"] = command.Username,
                            ["NEW_PASSWORD"] = command.NewPassword,
                            ["SECRET_HASH"] = GenerateSecretHash(
                                command.Username,
                                _options.Value.ClientId,
                                _options.Value.ClientSecret
                            ),
                        },
                    },
                    cancellationToken
                );

            AuthenticationResultType? auth = cognitoResponse?.AuthenticationResult;

            if (auth is null)
            {
                return UpdatePasswordResult.Err(new UpdatePasswordError.Unauthorised());
            }

            var response = new AuthenticationCredentialsDto { AccessToken = auth.AccessToken };
            return UpdatePasswordResult.Ok(response);
        }
        catch (NotAuthorizedException)
        {
            return UpdatePasswordResult.Err(new UpdatePasswordError.Unauthorised());
        }
    }

    private static LoginError ConvertChallengeToError(AdminInitiateAuthResponse cognitoResponse)
    {
        ChallengeNameType challengeName = cognitoResponse.ChallengeName;
        return new Dictionary<ChallengeNameType, LoginError>()
        {
            [ChallengeNameType.NEW_PASSWORD_REQUIRED] = new LoginError.Challenge(
                UkpsChallengeType.NewPasswordRequired,
                cognitoResponse.Session
            ),
        }.TryGetValue(challengeName, out LoginError? err)
            ? err
            : throw new NotSupportedException($"Unhandled challenge type [{challengeName}].");
    }

    public static string GenerateSecretHash(string username, string clientId, string clientSecret)
    {
        var key = Encoding.UTF8.GetBytes(clientSecret);
        var message = Encoding.UTF8.GetBytes(username + clientId);

        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(message);

        return Convert.ToBase64String(hash);
    }
}
