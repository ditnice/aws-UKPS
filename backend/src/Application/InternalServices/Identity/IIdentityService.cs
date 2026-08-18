using UKPS.Api.Application.Authentication.Dtos;
using UKPS.Api.Persistence.Entities.Identity;
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

    Task<CreateNewUserResult> CreateNewUser(
        UserIdentityId userIdentityId,
        string email,
        CancellationToken cancellationToken
    );

    Task<InitiatedAuthenticationResult> InitiateAuthentication(
        UserIdentityId userIdentityId,
        string newPassword,
        CancellationToken cancellationToken
    );

    Task MarkEmailAsVerified(UserIdentityId userIdentityId, CancellationToken cancellationToken);
    Task<InitiatedAuthenticationResult> RefreshAuthenticationToken(
        string refreshToken,
        CancellationToken cancellationToken
    );
    Task<InitiatedAuthenticationResult> RespondToMultiFactorAuthenticationChallenge(
        UserIdentityId userIdentityId,
        string authenticationSession,
        string code,
        CancellationToken cancellationToken
    );

    Task<UpdatePasswordResult> UpdatePassword(
        UserIdentityId userIdentityId,
        string newPassword,
        CancellationToken cancellationToken
    );
    Task UpdateUserEmail(
        UserIdentityId userIdentityId,
        string updatedEmail,
        CancellationToken cancellationToken
    );

    Task<AuthenticationCredentialsDto> VerifySoftwareToken(
        UserIdentityId userIdentityId,
        string authenticationSessionId,
        string code,
        CancellationToken cancellationToken
    );
}
