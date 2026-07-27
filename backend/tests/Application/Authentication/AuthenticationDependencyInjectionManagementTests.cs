using Amazon.CognitoIdentityProvider;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using UKPS.Api.Application.Authentication;

namespace UKPS.Api.Tests.Application.Authentication;

public sealed class AuthenticationDependencyInjectionManagementTests
{
    [Fact]
    public void AddAuthenticationServices_ShouldRegisterAuthenticationService()
    {
        using var provider = CreateServiceProvider();

        var service = provider.GetService<IAuthenticationService>();

        service.ShouldNotBeNull();
        service.ShouldBeOfType<AuthenticationService>();
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
            x.ServiceType == typeof(IAuthenticationService)
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

        var authenticationService = Substitute.For<IAuthenticationService>();
        var cognito = Substitute.For<IAmazonCognitoIdentityProvider>();

        services.AddScoped(_ => authenticationService);
        services.AddScoped(_ => cognito);

        services.AddAuthenticationServices();

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IAuthenticationService>().ShouldBe(authenticationService);

        provider.GetRequiredService<IAmazonCognitoIdentityProvider>().ShouldBe(cognito);
    }

    private static ServiceCollection CreateServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton(Options.Create(CreateCognitoConfiguration()));

        services.AddAuthenticationServices();

        return services;
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = CreateServices();

        return services.BuildServiceProvider();
    }

    private static CognitoConfiguration CreateCognitoConfiguration() =>
        new()
        {
            ClientId = "client-id",
            Region = "eu-west-2",
            AccessKey = "access-key",
            SecretKey = "secret-key",
            ServiceUrl = new Uri("https://cognito.example.com"),
        };
}
