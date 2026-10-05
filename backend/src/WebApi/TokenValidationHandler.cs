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
            .Users.Include(x => x.UserOrgMemberships)!
                .ThenInclude(x => x.Organisation)
            .FirstOrDefaultAsync(x => x.CognitoUsername == CognitoUsername.Parse(username));

        if (user is null)
        {
            context.Fail(AuthenticationFailCode.NoDbUserExistsWithUsername.ToString());
            return;
        }

        var identity = context.Principal?.Identity as ClaimsIdentity;

        MembershipSelection selection = GetSelectedMembership(user, context);

        if (selection.SelectionFailure is { } selectionFailure)
        {
            // The user is authenticated but must select an organisation. Only endpoints using the
            // organisation selection policy are accessible until they have done so.
            identity?.AddClaim(new Claim(UkpsClaimTypes.Email, user.WorkEmail));
            identity?.AddClaim(
                new Claim(UkpsClaimTypes.OrganisationSelectionFailure, $"{selectionFailure}")
            );
            return;
        }

        if (selection.Membership is not { } membership)
        {
            return;
        }

        identity?.AddClaim(new Claim(UkpsClaimTypes.Email, user.WorkEmail));
        identity?.AddClaim(
            new Claim(UkpsClaimTypes.OrganisationId, $"{membership.OrganisationId}")
        );
        identity?.AddClaim(new Claim(UkpsClaimTypes.UserRole, $"{membership.UserRole}"));
    }

    private static MembershipSelection GetSelectedMembership(
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
        var memberships = user.UserOrgMemberships.ToArray();

        if (memberships.Length == 0)
        {
            context.Fail(AuthenticationFailCode.NoMembershipsForUser.ToString());
            return MembershipSelection.Failed;
        }

        if (memberships.Length == 1)
        {
            return new MembershipSelection(
                GuardMembershipIsSelectable(memberships.Single(), context)
            );
        }

        var selectedMembership = memberships.FirstOrDefault(x => x.IsSelectedAsCurrentOrganisation);

        if (selectedMembership is not null && selectedMembership.IsSelectable())
        {
            return new MembershipSelection(selectedMembership);
        }

        UserOrgMembership[] selectableMemberships = memberships
            .Where(x => x.IsSelectable())
            .ToArray();

        if (selectableMemberships.Length == 1)
        {
            // Only one organisation can be managed, so there is nothing for the user to select.
            return new MembershipSelection(selectableMemberships[0]);
        }

        if (selectableMemberships.Length > 1)
        {
            return new MembershipSelection(
                null,
                selectedMembership is null
                    ? AuthenticationFailCode.SelectedOrganisationRequired
                    : AuthenticationFailCode.SelectedOrganisationIsNotValid
            );
        }

        // None of the user's memberships are selectable, so selecting a different
        // organisation would not help. Fail using the most relevant membership.
        var failingMembership =
            selectedMembership
            ?? memberships.FirstOrDefault(x => x.Status == UserOrgMembershipStatus.Deactivated)
            ?? memberships[0];
        return new MembershipSelection(GuardMembershipIsSelectable(failingMembership, context));
    }

    private static UserOrgMembership? GuardMembershipIsSelectable(
        UserOrgMembership userOrgMembership,
        TokenValidatedContext context
    )
    {
        if (userOrgMembership.Status == UserOrgMembershipStatus.Deactivated)
        {
            context.Fail(AuthenticationFailCode.MembershipDeactivated.ToString());
            return null;
        }
        if (!userOrgMembership.IsAuthorised())
        {
            context.Fail(AuthenticationFailCode.MembershipNotInValidState.ToString());
            return null;
        }
        if (!userOrgMembership.IsOrganisationActive())
        {
            context.Fail(AuthenticationFailCode.OrganisationNotActive.ToString());
            return null;
        }

        return userOrgMembership;
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

    private readonly record struct MembershipSelection(
        UserOrgMembership? Membership,
        AuthenticationFailCode? SelectionFailure = null
    )
    {
        public static MembershipSelection Failed => new(null);
    }
}
