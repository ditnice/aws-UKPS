using Amazon.CognitoIdentityProvider;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using UKPS.Api.Application.Authentication;
using UKPS.Api.Application.InternalServices.Identity;

namespace UKPS.Api.Tests.Application.Authentication;

public sealed class AuthenticationDependencyInjectionManagementTests
{
    [Fact]
    public void AddAuthenticationServices_ShouldRegisterIdentityService()
    {
        using var provider = CreateServiceProvider();

        var service = provider.GetService<IIdentityService>();

        service.ShouldNotBeNull();
        service.ShouldBeOfType<CognitoIdentityService>();
    }

    [Fact]
    public void AddAuthenticationServices_ShouldRegisterAmazonCognitoIdentityProvider()
    {
        using var provider = CreateServiceProvider();

        var client = provider.GetService<IAmazonCognitoIdentityProvider>();

        client.ShouldNotBeNull();
        client.ShouldBeOfType<AmazonCognitoIdentityProviderClient>();
    }

    [Fact]
    public void AddAuthenticationServices_ShouldRegisterServicesAsScoped()
    {
        var services = CreateServices();

        var authenticationRegistration = services.Single(x =>
            x.ServiceType == typeof(IIdentityService)
        );

        authenticationRegistration.Lifetime.ShouldBe(ServiceLifetime.Scoped);

        var cognitoRegistration = services.Single(x =>
            x.ServiceType == typeof(IAmazonCognitoIdentityProvider)
        );

        cognitoRegistration.Lifetime.ShouldBe(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddAuthenticationServices_ShouldNotOverrideExistingRegistrations()
    {
        var services = CreateServices();

        var cognito = Substitute.For<IAmazonCognitoIdentityProvider>();

        services.AddScoped(_ => cognito);

        services.AddAuthenticationServices();

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IAmazonCognitoIdentityProvider>().ShouldBe(cognito);
    }

    [Fact]
    public void AddAuthenticationServices_ShouldDeriveServiceUrlFromRegionIfServiceRulIsNotSet()
    {
        var services = CreateServices(x => x with { ServiceUrlOverride = null });
        using var provider = services.BuildServiceProvider();

        CognitoOptions options = provider.GetRequiredService<IOptions<CognitoOptions>>().Value;
        var client = provider.GetRequiredService<IAmazonCognitoIdentityProvider>();
        client.Config.ServiceURL.ShouldBe($"https://cognito-idp.{options.Region}.amazonaws.com/");
    }

    private static ServiceCollection CreateServices(
        Func<CognitoOptions, CognitoOptions>? modifier = null
    )
    {
        var services = new ServiceCollection();

        var configuration = modifier is null
            ? CreateCognitoConfiguration()
            : modifier(CreateCognitoConfiguration());
        services.AddSingleton(Options.Create(configuration));

        services.AddAuthenticationServices().AddLogging(x => x.AddConsole());

        return services;
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = CreateServices();

        return services.BuildServiceProvider();
    }

    private static CognitoOptions CreateCognitoConfiguration() =>
        new()
        {
            ClientId = "client-id",
            Region = "region",
            ServiceUrlOverride = new Uri("https://cognito.example.com"),
            ClientSecret = "client-secret",
            UserPoolId = "user-pool-id",
        };
}
