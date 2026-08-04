using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using UKPS.Api.WebApi;

namespace UKPS.Api.Tests.WebApi;

public sealed class AwsAuthenticationExtensionsTests
{
    [Fact]
    public void AddAwsBearerAuthentication_WhenBuilderIsNull_ThrowsArgumentNullException()
    {
        WebApplicationBuilder? builder = null;

        Should.Throw<ArgumentNullException>(() => builder!.AddAwsBearerAuthentication());
    }

    [Fact]
    public void AddAwsBearerAuthentication_ConfiguresBearerAuthentication()
    {
        var expectedAuthorityAndIssuer =
            "https://cognito-idp.eu-west-2.amazonaws.com/eu-west-2_example";
        var builder = CreateBuilder();

        builder.AddAwsBearerAuthentication();

        using var serviceProvider = builder.Services.BuildServiceProvider();

        var optionsMonitor = serviceProvider.GetRequiredService<
            IOptionsMonitor<JwtBearerOptions>
        >();

        var options = optionsMonitor.Get(JwtBearerDefaults.AuthenticationScheme);

        options.Authority.ShouldBe(expectedAuthorityAndIssuer);

        options.TokenValidationParameters.ValidateLifetime.ShouldBeTrue();
        options.TokenValidationParameters.ValidateIssuer.ShouldBeTrue();
        options.TokenValidationParameters.ValidateIssuerSigningKey.ShouldBeTrue();
        options.TokenValidationParameters.ValidateAudience.ShouldBeFalse();

        options.TokenValidationParameters.ValidIssuer.ShouldBe(expectedAuthorityAndIssuer);

        options.Events.ShouldNotBeNull();
        options.Events.OnMessageReceived.ShouldNotBeNull();
        options.Events.OnTokenValidated.ShouldNotBeNull();
    }

    [Fact]
    public async Task OnMessageReceived_WhenAccessTokenCookieExists_UsesCookieAsToken()
    {
        var builder = CreateBuilder();
        builder.AddAwsBearerAuthentication();

        using var serviceProvider = builder.Services.BuildServiceProvider();

        var options = serviceProvider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Cookie = "access_token=the-access-token";

        var context = new MessageReceivedContext(
            httpContext,
            new AuthenticationScheme(
                JwtBearerDefaults.AuthenticationScheme,
                JwtBearerDefaults.AuthenticationScheme,
                typeof(JwtBearerHandler)
            ),
            options
        );

        await options.Events.OnMessageReceived(context);

        context.Token.ShouldBe("the-access-token");
    }

    [Fact]
    public async Task OnMessageReceived_WhenAccessTokenCookieDoesNotExist_SetsTokenToNull()
    {
        var builder = CreateBuilder();
        builder.AddAwsBearerAuthentication();

        using var serviceProvider = builder.Services.BuildServiceProvider();

        var options = serviceProvider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        var httpContext = new DefaultHttpContext();

        var context = new MessageReceivedContext(
            httpContext,
            new AuthenticationScheme(
                JwtBearerDefaults.AuthenticationScheme,
                JwtBearerDefaults.AuthenticationScheme,
                typeof(JwtBearerHandler)
            ),
            options
        );

        await options.Events.OnMessageReceived(context);

        context.Token.ShouldBeNull();
    }

    [Fact]
    public async Task OnTokenValidated_WhenTokenUseIsAccessAndClientIdMatches_DoesNotFail()
    {
        var builder = CreateBuilder();
        builder.AddAwsBearerAuthentication();

        using var serviceProvider = builder.Services.BuildServiceProvider();

        var options = serviceProvider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        var principal = CreatePrincipal(("token_use", "access"), ("client_id", "test-client-id"));

        var context = CreateTokenValidatedContext(options, principal);

        await options.Events.OnTokenValidated(context);

        context.Result.ShouldBeNull();
    }

    [Fact]
    public async Task OnTokenValidated_WhenTokenUseIsNotAccess_FailsAuthentication()
    {
        var builder = CreateBuilder();
        builder.AddAwsBearerAuthentication();

        using var serviceProvider = builder.Services.BuildServiceProvider();

        var options = serviceProvider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        var principal = CreatePrincipal(("token_use", "id"), ("client_id", "test-client-id"));

        var context = CreateTokenValidatedContext(options, principal);

        await options.Events.OnTokenValidated(context);

        context.Result.ShouldNotBeNull();
        context.Result.Failure.ShouldNotBeNull();
        context.Result.Failure.Message.ShouldBe("Token is not an access token.");
    }

    [Fact]
    public async Task OnTokenValidated_WhenTokenUseClaimIsMissing_FailsAuthentication()
    {
        var builder = CreateBuilder();
        builder.AddAwsBearerAuthentication();

        using var serviceProvider = builder.Services.BuildServiceProvider();

        var options = serviceProvider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        var principal = CreatePrincipal(("client_id", "test-client-id"));

        var context = CreateTokenValidatedContext(options, principal);

        await options.Events.OnTokenValidated(context);

        context.Result.ShouldNotBeNull();
        context.Result.Failure.ShouldNotBeNull();
        context.Result.Failure.Message.ShouldBe("Token is not an access token.");
    }

    [Fact]
    public async Task OnTokenValidated_WhenClientIdDoesNotMatch_FailsAuthentication()
    {
        var builder = CreateBuilder();
        builder.AddAwsBearerAuthentication();

        using var serviceProvider = builder.Services.BuildServiceProvider();

        var options = serviceProvider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        var principal = CreatePrincipal(("token_use", "access"), ("client_id", "wrong-client-id"));

        var context = CreateTokenValidatedContext(options, principal);

        await options.Events.OnTokenValidated(context);

        context.Result.ShouldNotBeNull();
        context.Result.Failure.ShouldNotBeNull();
        context.Result.Failure.Message.ShouldBe("Token was not issued to the expected client.");
    }

    [Fact]
    public async Task OnTokenValidated_WhenClientIdClaimIsMissing_FailsAuthentication()
    {
        var builder = CreateBuilder();
        builder.AddAwsBearerAuthentication();

        using var serviceProvider = builder.Services.BuildServiceProvider();

        var options = serviceProvider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        var principal = CreatePrincipal(("token_use", "access"));

        var context = CreateTokenValidatedContext(options, principal);

        await options.Events.OnTokenValidated(context);

        context.Result.ShouldNotBeNull();
        context.Result.Failure.ShouldNotBeNull();
        context.Result.Failure.Message.ShouldBe("Token was not issued to the expected client.");
    }

    private static WebApplicationBuilder CreateBuilder()
    {
        var builder = WebApplication.CreateBuilder();

        builder.Configuration.AddInMemoryCollection(
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["cognito:ServiceUrl"] = "https://cognito.eu-west-2.amazonaws.com/",
                ["cognito:UserPoolId"] = "eu-west-2_example",
                ["cognito:ClientId"] = "test-client-id",
                ["cognito:ClientSecret"] = "test-client-secret",
                ["cognito:Region"] = "eu-west-2",
            }
        );

        return builder;
    }

    private static ClaimsPrincipal CreatePrincipal(params (string Type, string Value)[] claims)
    {
        return new ClaimsPrincipal(
            new ClaimsIdentity(
                claims.Select(x => new Claim(x.Type, x.Value)),
                authenticationType: "Test"
            )
        );
    }

    private static TokenValidatedContext CreateTokenValidatedContext(
        JwtBearerOptions options,
        ClaimsPrincipal principal
    )
    {
        var httpContext = new DefaultHttpContext();

        var scheme = new AuthenticationScheme(
            JwtBearerDefaults.AuthenticationScheme,
            JwtBearerDefaults.AuthenticationScheme,
            typeof(JwtBearerHandler)
        );

        return new TokenValidatedContext(httpContext, scheme, options) { Principal = principal };
    }
}
