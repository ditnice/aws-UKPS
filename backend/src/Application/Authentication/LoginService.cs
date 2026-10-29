using Microsoft.EntityFrameworkCore;
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
        var user = await GetUserWithMemberships(request.Username, cancellationToken);
        if (user is null)
        {
            return InitiateAuthenticationResult.Err(new InitiateAuthenticationError.Unauthorised());
        }
        var result = await _identityService.InitiateAuthentication(
            user.CognitoUsername,
            request.Password,
            cancellationToken
        );
        if (result.IsOk)
        {
            await ResetCurrentOrganisation(user, cancellationToken);
        }
        return result;
    }

    public async Task<InitiateAuthenticationResult> RespondToMultiFactorAuthenticationChallenge(
        RespondToMultiFactorAuthenticationChallengeCommand command,
        CancellationToken cancellationToken
    )
    {
        var user = await GetUserWithMemberships(command.Username, cancellationToken);
        if (user is null)
        {
            return InitiateAuthenticationResult.Err(new InitiateAuthenticationError.Unauthorised());
        }
        var result = await _identityService.RespondToMultiFactorAuthenticationChallenge(
            user.CognitoUsername,
            command.AuthenticationSession,
            command.Code,
            cancellationToken
        );
        if (result.IsOk)
        {
            await ResetCurrentOrganisation(user, cancellationToken);
        }
        return result;
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

    private Task<User?> GetUserWithMemberships(string email, CancellationToken cancellationToken) =>
        _appDbContext
            .Users.Include(x => x.UserOrgMemberships)!
                .ThenInclude(x => x.Organisation)
            .GetByEmailOrDefault(email, cancellationToken);

    // Each sign-in clears the selected organisation, then selects it again only when there is
    // just one to choose. If the flag is left null (for users with multiple memberships), they
    // are routed to the organisation picker page.
    private async Task ResetCurrentOrganisation(User user, CancellationToken cancellationToken)
    {
        user.ResetCurrentOrganisation();
        await _appDbContext.SaveChangesAsync(cancellationToken);

        int? soleOrganisationId = user.FindSoleSelectableOrganisationId();
        if (soleOrganisationId.HasValue)
        {
            user.TryUpdateCurrentOrganisation(soleOrganisationId.Value);
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
