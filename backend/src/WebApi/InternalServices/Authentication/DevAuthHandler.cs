using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.WebApi.InternalServices.Authentication;

internal class DevAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthenticationScheme = "DevScheme";
    private readonly DevAuthenticationClaims _authOptions;

    public DevAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        DevAuthenticationClaims authOptions
    )
        : base(options, logger, encoder)
    {
        _authOptions = authOptions;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = new ClaimsIdentity(GetClaims(), AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    /// <summary>
    /// The configured claims, or the claims of the role named by the dev role cookie.
    /// </summary>
    private IEnumerable<Claim> GetClaims()
    {
        string? requestedRole = Request.Cookies[DevAuthenticationClaims.RoleCookieName];
        // Matched by name only, so a number such as "2" doesn't pick a role.
        string? roleName = Enum.GetNames<UserRole>()
            .FirstOrDefault(name =>
                string.Equals(name, requestedRole, StringComparison.OrdinalIgnoreCase)
            );

        return roleName is null
            ? _authOptions.Claims
            : _authOptions.ForRole(Enum.Parse<UserRole>(roleName));
    }
}
