using System.Security.Cryptography;
using System.Text;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Options;
using UKPS.Api.Application.Authentication;
using UKPS.Api.Application.Authentication.Dtos;
using UKPS.Api.Application.Authentication.Errors;
using CreateNewUserResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.InternalServices.Identity.CreateNewUserError>;
using InitiatedAuthenticationResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.AuthenticationCredentialsDto,
    UKPS.Api.Application.InternalServices.Identity.InitiateAuthenticationError
>;
using UpdatePasswordResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.InternalServices.Identity.UpdatePasswordError>;

namespace UKPS.Api.Application.InternalServices.Identity;

internal class CognitoWebIdentityAdministrationService
{
    private readonly IAmazonCognitoIdentityProvider _cognito;
    private readonly IOptions<CognitoConfiguration> _options;

    public CognitoWebIdentityAdministrationService(
        IAmazonCognitoIdentityProvider cognito,
        IOptions<CognitoConfiguration> options
    )
    {
        _cognito = cognito;
        _options = options;
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
            await _cognito.AdminCreateUserAsync(request, cancellationToken);
        }
        catch (UsernameExistsException)
        {
            return CreateNewUserResult.Err(new CreateNewUserError.UsernameAlreadyExists());
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to create a new user.", ex);
        }

        return CreateNewUserResult.Ok();
    }

    internal async Task<UpdatePasswordResult> UpdatePassword(
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
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to update the user's password.", ex);
        }

        return UpdatePasswordResult.Ok();
    }

    internal async Task<InitiatedAuthenticationResult> InitiateAuthentication(
        string userEmail,
        string newPassword,
        CancellationToken cancellationToken
    )
    {
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

            if (cognitoResponse?.ChallengeName is not null)
            {
                return InitiatedAuthenticationResult.Err(ConvertChallengeToError(cognitoResponse));
            }

            var auth = cognitoResponse?.AuthenticationResult;

            if (auth is null)
            {
                return InitiatedAuthenticationResult.Err(
                    new InitiateAuthenticationError.Unauthorised()
                );
            }

            var response = new AuthenticationCredentialsDto { AccessToken = auth.AccessToken };
            return InitiatedAuthenticationResult.Ok(response);
        }
        catch (NotAuthorizedException)
        {
            return InitiatedAuthenticationResult.Err(
                new InitiateAuthenticationError.Unauthorised()
            );
        }
    }

    internal async Task<AssociateSoftwareTokenResult> AssociateSoftwareToken(
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
            AuthenticationSessionId = associateResponse.Session,
        };
    }

    private static InitiateAuthenticationError ConvertChallengeToError(
        AdminInitiateAuthResponse cognitoResponse
    )
    {
        var lookup = new Dictionary<ChallengeNameType, InitiateAuthenticationError>()
        {
            [ChallengeNameType.MFA_SETUP] = new InitiateAuthenticationError.Challenge(
                UkpsChallengeType.MultiFactorAuthenticationSetupRequired,
                cognitoResponse.Session
            ),
            [ChallengeNameType.SOFTWARE_TOKEN_MFA] = new InitiateAuthenticationError.Challenge(
                UkpsChallengeType.MultiFactorAuthenticationRequired,
                cognitoResponse.Session
            ),
        };

        ChallengeNameType challengeName = cognitoResponse.ChallengeName;
        return lookup.TryGetValue(challengeName, out InitiateAuthenticationError? err)
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
