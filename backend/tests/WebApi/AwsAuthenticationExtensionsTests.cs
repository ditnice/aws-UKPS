using System.Security.Claims;
using Bogus;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using UKPS.Api.Application.Authentication;
using UKPS.Api.Persistence;
using UKPS.Api.WebApi;
using UKPS.Api.WebApi.InternalServices.Authentication;

namespace UKPS.Api.Tests.WebApi;

public sealed class AwsAuthenticationExtensionsTests
{
    [Fact]
    public void AddAwsBearerAuthentication_ShouldThrow_WhenBuilderIsNull()
    {
        WebApplicationBuilder? builder = null;

        Should.Throw<ArgumentNullException>(() => builder!.AddAwsBearerAuthentication());
    }

    [Fact]
    public async Task AddAwsBearerAuthentication_ShouldRegisterDevAuthentication_WhenDevAuthenticationIsEnabled()
    {
        var builder = CreateBuilder(devAuthenticationOptions: true);

        builder.AddAwsBearerAuthentication();

        using var provider = builder.Services.BuildServiceProvider();

        var schemeProvider = provider.GetRequiredService<IAuthenticationSchemeProvider>();

        var scheme = await schemeProvider.GetSchemeAsync(DevAuthHandler.AuthenticationScheme);

        scheme.ShouldNotBeNull();
        scheme.HandlerType.ShouldBe(typeof(DevAuthHandler));

        var options = provider.GetService<DevAuthenticationClaims>();

        options.ShouldNotBeNull();
    }

    [Fact]
    public async Task AddAwsBearerAuthentication_ShouldNotRegisterJwtAuthentication_WhenDevAuthenticationIsEnabled()
    {
        var builder = CreateBuilder(devAuthenticationOptions: true);

        builder.AddAwsBearerAuthentication();

        using var provider = builder.Services.BuildServiceProvider();

        var schemeProvider = provider.GetRequiredService<IAuthenticationSchemeProvider>();

        var bearerScheme = await schemeProvider.GetSchemeAsync(
            JwtBearerDefaults.AuthenticationScheme
        );

        bearerScheme.ShouldBeNull();
    }

    [Fact]
    public async Task AddAwsBearerAuthentication_ShouldRegisterStandardAuthentication_WhenDevAuthenticationIsDisabled()
    {
        var builder = CreateBuilder();

        builder.AddAwsBearerAuthentication();

        using var provider = builder.Services.AddDbContext<AppDbContext>().BuildServiceProvider();

        var schemeProvider = provider.GetRequiredService<IAuthenticationSchemeProvider>();

        var scheme = await schemeProvider.GetSchemeAsync(JwtBearerDefaults.AuthenticationScheme);

        scheme.ShouldNotBeNull();
        scheme.HandlerType.ShouldBe(typeof(JwtBearerHandler));

        provider.GetService<ITokenValidationHandler>().ShouldNotBeNull();
    }

    [Fact]
    public async Task AddAwsBearerAuthentication_ShouldRegisterStandardAuthentication_WhenDevAuthenticationConfigurationIsMissing()
    {
        var builder = CreateBuilder();

        builder.AddAwsBearerAuthentication();

        using var provider = builder.Services.BuildServiceProvider();

        var schemeProvider = provider.GetRequiredService<IAuthenticationSchemeProvider>();

        var scheme = await schemeProvider.GetSchemeAsync(JwtBearerDefaults.AuthenticationScheme);

        scheme.ShouldNotBeNull();
        scheme.HandlerType.ShouldBe(typeof(JwtBearerHandler));
    }

    [Fact]
    public void AddAwsBearerAuthentication_ShouldConfigureJwtBearerOptions()
    {
        var serviceUrl = new Uri("https://cognito-idp.eu-west-2.amazonaws.com/");

        var userPoolId = "eu-west-2_example";
        var expectedAuthority = new Uri(serviceUrl, userPoolId).AbsoluteUri;

        var builder = CreateBuilder(x =>
            x with
            {
                ServiceUrlOverride = serviceUrl,
                UserPoolId = userPoolId,
            }
        );

        builder.AddAwsBearerAuthentication();

        using var provider = builder.Services.BuildServiceProvider();

        var optionsMonitor = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>();

        var options = optionsMonitor.Get(JwtBearerDefaults.AuthenticationScheme);

        options.Authority.ShouldBe(expectedAuthority);

        options.TokenValidationParameters.ValidateLifetime.ShouldBeTrue();
        options.TokenValidationParameters.ValidateIssuer.ShouldBeTrue();
        options.TokenValidationParameters.ValidIssuer.ShouldBe(expectedAuthority);
        options.TokenValidationParameters.ValidateIssuerSigningKey.ShouldBeTrue();
        options.TokenValidationParameters.ValidateAudience.ShouldBeFalse();

        options.Events.ShouldNotBeNull();
        options.Events.OnMessageReceived.ShouldNotBeNull();
        options.Events.OnTokenValidated.ShouldNotBeNull();
    }

    [Fact]
    public async Task OnMessageReceived_ShouldReadAccessTokenFromCookie()
    {
        var builder = CreateBuilder();

        builder.AddAwsBearerAuthentication();

        using var provider = builder.Services.BuildServiceProvider();

        var options = provider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        var httpContext = new DefaultHttpContext { RequestServices = provider };

        httpContext.Request.Headers.Cookie = "access_token=the-jwt-token";

        var scheme = new AuthenticationScheme(
            JwtBearerDefaults.AuthenticationScheme,
            JwtBearerDefaults.AuthenticationScheme,
            typeof(JwtBearerHandler)
        );

        var context = new MessageReceivedContext(httpContext, scheme, options);

        await options.Events.OnMessageReceived(context);

        context.Token.ShouldBe("the-jwt-token");
    }

    [Fact]
    public async Task OnMessageReceived_ShouldSetTokenToNull_WhenAccessTokenCookieIsMissing()
    {
        var builder = CreateBuilder();

        builder.AddAwsBearerAuthentication();

        using var provider = builder.Services.BuildServiceProvider();

        var options = provider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        var httpContext = new DefaultHttpContext { RequestServices = provider };

        var scheme = new AuthenticationScheme(
            JwtBearerDefaults.AuthenticationScheme,
            JwtBearerDefaults.AuthenticationScheme,
            typeof(JwtBearerHandler)
        );

        var context = new MessageReceivedContext(httpContext, scheme, options);

        await options.Events.OnMessageReceived(context);

        context.Token.ShouldBeNull();
    }

    [Fact]
    public async Task OnTokenValidated_ShouldDelegateToTokenValidationHandler()
    {
        var tokenValidationHandler = Substitute.For<ITokenValidationHandler>();

        var builder = CreateBuilder();

        builder.Services.AddSingleton(tokenValidationHandler);

        builder.AddAwsBearerAuthentication();

        using var provider = builder.Services.BuildServiceProvider();

        var cancellationToken = CancellationToken.None;
        var httpContext = new DefaultHttpContext
        {
            RequestServices = provider,
            RequestAborted = cancellationToken,
        };

        var options = provider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        var scheme = new AuthenticationScheme(
            JwtBearerDefaults.AuthenticationScheme,
            JwtBearerDefaults.AuthenticationScheme,
            typeof(JwtBearerHandler)
        );

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[] { new Claim("username", "test-user") },
                JwtBearerDefaults.AuthenticationScheme
            )
        );

        var context = new TokenValidatedContext(httpContext, scheme, options)
        {
            Principal = principal,
        };

        await options.Events.OnTokenValidated(context);

        await tokenValidationHandler.Received(1).Handle(context, cancellationToken);
    }

    [Fact]
    public void AddAwsBearerAuthentication_ShouldThrow_WhenCognitoConfigurationIsMissing()
    {
        var builder = CreateBuilder(x => null!);

        builder.AddAwsBearerAuthentication();

        using var provider = builder.Services.BuildServiceProvider();

        var exception = Should.Throw<InvalidOperationException>(() =>
            provider
                .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
                .Get(JwtBearerDefaults.AuthenticationScheme)
        );

        exception.Message.ShouldBe(
            $"Jwt configuration section [{CognitoOptions.SectionName}] is missing or invalid."
        );
    }

    private static WebApplicationBuilder CreateBuilder(
        Func<CognitoOptions, CognitoOptions>? cognitoOptionsModifier = null,
        bool devAuthenticationOptions = false
    )
    {
        var fakeOptions = new CognitoOptionsFaker().Generate();
        var options = cognitoOptionsModifier is not null
            ? cognitoOptionsModifier(fakeOptions)
            : fakeOptions;
        var builder = WebApplication.CreateBuilder();

        var devAuthConfig = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            [
                $"{DevAuthenticationOptions.SectionName}:{nameof(DevAuthenticationOptions.IsEnabled)}"
            ] = devAuthenticationOptions.ToString(),
        };

        if (options is null)
        {
            builder.Configuration.AddInMemoryCollection(devAuthConfig);
            return builder;
        }
        builder.Configuration.AddInMemoryCollection(
            new Dictionary<string, string?>(devAuthConfig, StringComparer.Ordinal)
            {
                [$"{CognitoOptions.SectionName}:{nameof(CognitoOptions.ClientId)}"] =
                    options.ClientId,
                [$"{CognitoOptions.SectionName}:{nameof(CognitoOptions.ClientSecret)}"] =
                    options.ClientSecret,
                [$"{CognitoOptions.SectionName}:{nameof(CognitoOptions.Region)}"] = options.Region,
                [$"{CognitoOptions.SectionName}:{nameof(CognitoOptions.UserPoolId)}"] =
                    options.UserPoolId,
                [$"{CognitoOptions.SectionName}:{nameof(CognitoOptions.ServiceUrlOverride)}"] =
                    options.ServiceUrlOverride?.AbsoluteUri,
            }
        );

        return builder;
    }

    private sealed class CognitoOptionsFaker : Faker<CognitoOptions>
    {
        public CognitoOptionsFaker()
        {
            RuleFor(x => x.ServiceUrlOverride, f => null);
            RuleFor(x => x.ClientId, f => f.Random.AlphaNumeric(26));
            RuleFor(x => x.ClientSecret, f => f.Random.AlphaNumeric(40));
            RuleFor(
                x => x.Region,
                f => f.PickRandom("eu-west-1", "eu-west-2", "eu-central-1", "us-east-1")
            );

            RuleFor(
                x => x.UserPoolId,
                (f, options) => $"{options.Region}_{f.Random.AlphaNumeric(9)}"
            );
        }
    }
}
