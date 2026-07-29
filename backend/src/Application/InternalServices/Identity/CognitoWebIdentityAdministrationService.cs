using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Options;
using UKPS.Api.Application.Authentication;
using CreateNewUserResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.InternalServices.Identity.CreateNewUserError>;
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
}
