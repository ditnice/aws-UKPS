using UKPS.Api.Application.Authentication.Dtos;
using UKPS.Api.Persistence.Entities.Identity;
using CreateNewUserResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.InternalServices.Identity.CreateNewUserError>;
using InitiatedAuthenticationResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.AuthenticationCredentialsDto,
    UKPS.Api.Application.InternalServices.Identity.InitiateAuthenticationError
>;
using UpdatePasswordResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.InternalServices.Identity.UpdatePasswordError>;

namespace UKPS.Api.Application.InternalServices.Identity;

internal interface IIdentityService
{
    Task<AssociateSoftwareTokenResult> AssociateSoftwareToken(
        string authenticationSessionId,
        CancellationToken cancellationToken
    );

    Task<CreateNewUserResult> CreateNewUser(
        CognitoUsername cognitoUsername,
        string email,
        CancellationToken cancellationToken
    );

    Task<InitiatedAuthenticationResult> InitiateAuthentication(
        CognitoUsername cognitoUsername,
        string newPassword,
        CancellationToken cancellationToken
    );

    Task MarkEmailAsVerified(CognitoUsername cognitoUsername, CancellationToken cancellationToken);
    Task<InitiatedAuthenticationResult> RefreshAuthenticationToken(
        string refreshToken,
        CancellationToken cancellationToken
    );
    Task<InitiatedAuthenticationResult> RespondToMultiFactorAuthenticationChallenge(
        CognitoUsername cognitoUsername,
        string authenticationSession,
        string code,
        CancellationToken cancellationToken
    );

    Task<UpdatePasswordResult> UpdatePassword(
        CognitoUsername cognitoUsername,
        string newPassword,
        CancellationToken cancellationToken
    );
    Task UpdateUserEmail(
        CognitoUsername cognitoUsername,
        string updatedEmail,
        CancellationToken cancellationToken
    );

    Task<AuthenticationCredentialsDto> VerifySoftwareToken(
        CognitoUsername cognitoUsername,
        string authenticationSessionId,
        string code,
        CancellationToken cancellationToken
    );
}
