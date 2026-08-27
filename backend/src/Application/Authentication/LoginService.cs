using UKPS.Api.Application.Authentication.Dtos;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.Identity;
using InitiateAuthenticationResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.AuthenticationCredentialsDto,
    UKPS.Api.Application.InternalServices.Identity.InitiateAuthenticationError
>;

namespace UKPS.Api.Application.Authentication;

internal class LoginService : ILoginService
{
    private readonly IIdentityService _identityService;
    private readonly AppDbContext _appDbContext;

    public LoginService(IIdentityService identityService, AppDbContext appDbContext)
    {
        _identityService = identityService;
        _appDbContext = appDbContext;
    }

    public async Task<InitiateAuthenticationResult> Login(
        LoginRequest request,
        CancellationToken cancellationToken
    )
    {
        var user = await _appDbContext.Users.GetByEmailOrDefault(
            request.Username,
            cancellationToken
        );
        if (user is null)
        {
            return InitiateAuthenticationResult.Err(new InitiateAuthenticationError.Unauthorised());
        }
        return await _identityService.InitiateAuthentication(
            user.CognitoUsername,
            request.Password,
            cancellationToken
        );
    }

    public async Task<InitiateAuthenticationResult> RespondToMultiFactorAuthenticationChallenge(
        RespondToMultiFactorAuthenticationChallengeCommand command,
        CancellationToken cancellationToken
    )
    {
        var user = await _appDbContext.Users.GetByEmailOrDefault(
            command.Username,
            cancellationToken
        );
        if (user is null)
        {
            return InitiateAuthenticationResult.Err(new InitiateAuthenticationError.Unauthorised());
        }
        return await _identityService.RespondToMultiFactorAuthenticationChallenge(
            user.CognitoUsername,
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
}
