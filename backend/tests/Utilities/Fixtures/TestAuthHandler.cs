using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace UKPS.Api.Tests.Utilities.Fixtures;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthenticationScheme = "TestScheme";
    private readonly TestAuthenticationOptions _authOptions;

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        TestAuthenticationOptions authOptions
    )
        : base(options, logger, encoder)
    {
        _authOptions = authOptions;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = new ClaimsIdentity(_authOptions.Claims);

        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), AuthenticationScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
