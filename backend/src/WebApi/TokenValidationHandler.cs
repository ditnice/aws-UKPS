using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UKPS.Api.Application.Authentication;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.WebApi.InternalServices.Identity;

namespace UKPS.Api.WebApi;

internal class TokenValidationHandler : ITokenValidationHandler
{
    private readonly AppDbContext _appDbContext;
    private readonly IOptions<CognitoOptions> _options;

    public TokenValidationHandler(AppDbContext appDbContext, IOptions<CognitoOptions> options)
    {
        _appDbContext = appDbContext;
        _options = options;
    }

    public async Task Handle(TokenValidatedContext context, CancellationToken cancellationToken)
    {
        var validationPasses = ValidateTokenUse(context) && ValidateClientId(context);
        if (validationPasses)
        {
            await AppendIdentityClaims(context);
        }
    }

    private async Task AppendIdentityClaims(TokenValidatedContext context)
    {
        var username =
            context.Principal?.FindFirst("username")?.Value
            ?? throw new InvalidOperationException(
                "Subject could not be found as expected on the JWT."
            );
        var user = await _appDbContext
            .Users.Include(x => x.UserOrgMemberships)
            .FirstOrDefaultAsync(x => x.CognitoUsername == CognitoUsername.Parse(username));

        if (user is null)
        {
            context.Fail(AuthenticationFailCode.NoDbUserExistsWithUsername.ToString());
            return;
        }

        var identity = context.Principal?.Identity as ClaimsIdentity;

        var membership = GetSelectedMembership(user, context);
        if (membership is null)
        {
            return;
        }

        identity?.AddClaim(new Claim(UkpsClaimTypes.Email, user.WorkEmail));
        identity?.AddClaim(
            new Claim(UkpsClaimTypes.OrganisationId, $"{membership.OrganisationId}")
        );
        identity?.AddClaim(new Claim(UkpsClaimTypes.UserRole, $"{membership.UserRole}"));
    }

    private static UserOrgMembership? GetSelectedMembership(
        User user,
        TokenValidatedContext context
    )
    {
        if (user.UserOrgMemberships is null)
        {
            throw new InvalidOperationException(
                "Cannot get users selected because the user's organisation memberships have not been loaded."
            );
        }
        var validMemberships = user.UserOrgMemberships.ToArray();

        if (validMemberships.Length == 0)
        {
            context.Fail(AuthenticationFailCode.NoMembershipsForUser.ToString());
            return null;
        }

        if (validMemberships.Length == 1)
        {
            return GuardMembershipIsActive(validMemberships.Single(), context);
        }

        var selectedOrganisationId = context.HttpContext.Request.Cookies["selected_organisation"];

        if (
            !int.TryParse(
                selectedOrganisationId,
                CultureInfo.InvariantCulture,
                out var organisationId
            )
        )
        {
            context.Fail(AuthenticationFailCode.SelectedOrganisationRequired.ToString());
            return null;
        }

        var membership = validMemberships.SingleOrDefault(x => x.OrganisationId == organisationId);

        if (membership is null)
        {
            context.Fail(AuthenticationFailCode.SelectedOrganisationIsNotValid.ToString());
            return null;
        }

        return GuardMembershipIsActive(membership, context);
    }

    private static UserOrgMembership? GuardMembershipIsActive(
        UserOrgMembership userOrgMembership,
        TokenValidatedContext context
    )
    {
        if (userOrgMembership.IsAuthorised())
        {
            return userOrgMembership;
        }
        if (userOrgMembership.Status == UserOrgMembershipStatus.Deactivated)
        {
            context.Fail(AuthenticationFailCode.MembershipDeactivated.ToString());
            return null;
        }

        context.Fail(AuthenticationFailCode.MembershipNotInValidState.ToString());
        return null;
    }

    private static bool ValidateTokenUse(TokenValidatedContext context)
    {
        var tokenUse = context.Principal?.FindFirst("token_use")?.Value;
        if (!string.Equals(tokenUse, "access", StringComparison.Ordinal))
        {
            context.Fail("Token is not an access token.");
            return false;
        }
        return true;
    }

    private bool ValidateClientId(TokenValidatedContext context)
    {
        var clientId = context.Principal?.FindFirst("client_id")?.Value;

        if (!string.Equals(_options.Value.ClientId, clientId, StringComparison.Ordinal))
        {
            context.Fail("Token was not issued to the expected client.");
            return false;
        }
        return true;
    }
}
