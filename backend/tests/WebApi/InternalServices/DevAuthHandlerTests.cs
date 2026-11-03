using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.WebApi.InternalServices.Authentication;
using UKPS.Api.WebApi.InternalServices.Identity;

namespace UKPS.Api.Tests.WebApi.InternalServices;

public sealed class DevAuthHandlerTests
{
    [Fact]
    public async Task Authenticate_WithoutRoleCookie_ShouldUseConfiguredClaims()
    {
        var configured = new DevAuthenticationClaims().UpdateClaim(
            new Claim(UkpsClaimTypes.UserRole, UserRole.Champion.ToString())
        );

        ClaimsPrincipal principal = await AuthenticateAsync(configured, roleCookie: null);

        principal.FindFirstValue(UkpsClaimTypes.UserRole).ShouldBe("Champion");
        principal
            .FindFirstValue(UkpsClaimTypes.Email)
            .ShouldBe(DevAuthenticationClaims.DefaultUserEmail);
    }

    [Theory]
    [InlineData("Super", UserRole.Super)]
    [InlineData("Champion", UserRole.Champion)]
    [InlineData("standard", UserRole.Standard)]
    public async Task Authenticate_WithRoleCookie_ShouldActAsThatRolesUser(
        string cookieValue,
        UserRole expectedRole
    )
    {
        DevAuthenticationUser expectedUser = DevAuthenticationClaims.RoleUsers[expectedRole];

        ClaimsPrincipal principal = await AuthenticateAsync(
            new DevAuthenticationClaims(),
            cookieValue
        );

        principal.FindFirstValue(UkpsClaimTypes.UserRole).ShouldBe(expectedRole.ToString());
        principal.FindFirstValue(UkpsClaimTypes.Email).ShouldBe(expectedUser.Email);
        principal
            .FindFirstValue(UkpsClaimTypes.Username)
            .ShouldBe(expectedUser.CognitoUsername.Value);
        principal.FindFirstValue(UkpsClaimTypes.OrganisationId).ShouldBe("1");
        principal.FindAll(UkpsClaimTypes.UserRole).Count().ShouldBe(1);
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("1")]
    [InlineData("")]
    public async Task Authenticate_WithUnknownRoleCookie_ShouldUseConfiguredClaims(
        string cookieValue
    )
    {
        ClaimsPrincipal principal = await AuthenticateAsync(
            new DevAuthenticationClaims(),
            cookieValue
        );

        principal.FindFirstValue(UkpsClaimTypes.UserRole).ShouldBe("Super");
        principal
            .FindFirstValue(UkpsClaimTypes.Email)
            .ShouldBe(DevAuthenticationClaims.DefaultUserEmail);
    }

    private static async Task<ClaimsPrincipal> AuthenticateAsync(
        DevAuthenticationClaims claims,
        string? roleCookie
    )
    {
        var optionsMonitor = Substitute.For<IOptionsMonitor<AuthenticationSchemeOptions>>();
        optionsMonitor.Get(Arg.Any<string>()).Returns(new AuthenticationSchemeOptions());
        var handler = new DevAuthHandler(
            optionsMonitor,
            NullLoggerFactory.Instance,
            UrlEncoder.Default,
            claims
        );
        var context = new DefaultHttpContext();
        if (roleCookie is not null)
        {
            context.Request.Headers.Cookie =
                $"{DevAuthenticationClaims.RoleCookieName}={roleCookie}";
        }

        await handler.InitializeAsync(
            new AuthenticationScheme(
                DevAuthHandler.AuthenticationScheme,
                displayName: null,
                typeof(DevAuthHandler)
            ),
            context
        );
        AuthenticateResult result = await handler.AuthenticateAsync();

        result.Succeeded.ShouldBeTrue();
        return result.Principal;
    }
}
