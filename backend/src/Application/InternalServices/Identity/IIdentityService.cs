using UKPS.Api.Application.Authentication.Dtos;
using CreateNewUserResult = UKPS.Api.Application.Common.Result<
    string,
    UKPS.Api.Application.InternalServices.Identity.CreateNewUserError
>;
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

    Task<CreateNewUserResult> CreateNewUser(string email, CancellationToken cancellationToken);

    Task<InitiatedAuthenticationResult> InitiateAuthentication(
        string userEmail,
        string newPassword,
        CancellationToken cancellationToken
    );

    Task MarkEmailAsVerified(string username, CancellationToken cancellationToken);
    Task<InitiatedAuthenticationResult> RefreshAuthenticationToken(
        string refreshToken,
        CancellationToken cancellationToken
    );
    Task<InitiatedAuthenticationResult> RespondToMultiFactorAuthenticationChallenge(
        string username,
        string authenticationSession,
        string code,
        CancellationToken cancellationToken
    );

    Task<UpdatePasswordResult> UpdatePassword(
        string userEmail,
        string newPassword,
        CancellationToken cancellationToken
    );
    Task UpdateUserEmail(
        string currentEmail,
        string updatedEmail,
        CancellationToken cancellationToken
    );

    Task<AuthenticationCredentialsDto> VerifySoftwareToken(
        string username,
        string authenticationSessionId,
        string code,
        CancellationToken cancellationToken
    );
}
