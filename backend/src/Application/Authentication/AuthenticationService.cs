using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Options;
using UKPS.Api.Application.Authentication.Dtos;
using UKPS.Api.Application.Authentication.Errors;
using LoginResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.AuthenticationCredentialsDto,
    UKPS.Api.Application.Authentication.Errors.LoginError
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
            var cognitoResponse = await _cognito.InitiateAuthAsync(
                new InitiateAuthRequest
                {
                    ClientId = _options.Value.ClientId,
                    AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
                    AuthParameters = new Dictionary<string, string>(StringComparer.InvariantCulture)
                    {
                        ["USERNAME"] = loginRequest.Username,
                        ["PASSWORD"] = loginRequest.Password,
                    },
                },
                cancellationToken
            );

            var auth = cognitoResponse.AuthenticationResult;

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
}
