using Amazon;
using Amazon.CognitoIdentityProvider;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace UKPS.Api.Application.Authentication;

internal static class AuthenticationDependencyInjectionManagement
{
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
    {
        services.TryAddScoped<IAuthenticationService, AuthenticationService>();
        services.TryAddScoped<IAmazonCognitoIdentityProvider>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<CognitoConfiguration>>().Value;

            var config = new AmazonCognitoIdentityProviderConfig
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region),
                ServiceURL = options.ServiceUrl?.AbsoluteUri,
            };

            return new AmazonCognitoIdentityProviderClient(
                options.AccessKey,
                options.SecretKey,
                config
            );
        });
        return services;
    }
}
