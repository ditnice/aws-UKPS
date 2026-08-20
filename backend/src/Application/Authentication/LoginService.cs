using UKPS.Api.Application.Authentication.Dtos;
using UKPS.Api.Application.InternalServices.Identity;
using InitiateAuthenticationResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.AuthenticationCredentialsDto,
    UKPS.Api.Application.InternalServices.Identity.InitiateAuthenticationError
>;

namespace UKPS.Api.Application.Authentication;

internal class LoginService : ILoginService
{
    private readonly IIdentityService _identityService;

    public LoginService(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<InitiateAuthenticationResult> Login(
        LoginRequest request,
        CancellationToken cancellationToken
    )
    {
        return _identityService.InitiateAuthentication(
            request.Username,
            request.Password,
            cancellationToken
        );
    }

    public Task<InitiateAuthenticationResult> RespondToMultiFactorAuthenticationChallenge(
        RespondToMultiFactorAuthenticationChallengeCommand command,
        CancellationToken cancellationToken
    )
    {
        return _identityService.RespondToMultiFactorAuthenticationChallenge(
            command.Username,
            command.AuthenticationSession,
            command.Code,
            cancellationToken
        );
    }

    public Task<InitiateAuthenticationResult> RefreshAuthenticationToken(
        RefreshAuthenticationTokenCommand command,
        CancellationToken cancellationToken
    )
    {
        return _identityService.RefreshAuthenticationToken(command.RefreshToken, cancellationToken);
    }

    public Task SignOut(string refreshToken, CancellationToken cancellationToken)
    {
        return string.IsNullOrWhiteSpace(refreshToken)
            ? Task.CompletedTask
            : _identityService.RevokeRefreshToken(refreshToken, cancellationToken);
    }
}
