using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Options;
using UKPS.Api.Application.Authentication;
using UKPS.Api.Application.Common;

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

    public async Task<Result<CreateNewUserError>> CreateNewUser(
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
            return Result<CreateNewUserError>.Err(new CreateNewUserError.UsernameAlreadyExists());
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to create a new user.", ex);
        }

        return Result<CreateNewUserError>.Ok();
    }
}
