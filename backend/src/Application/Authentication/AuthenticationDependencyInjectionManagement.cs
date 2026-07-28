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
            CognitoConfiguration options = sp.GetRequiredService<
                IOptions<CognitoConfiguration>
            >().Value;
            string serviceUrl =
                options.ServiceUrl?.AbsoluteUri
                ?? $"https://cognito-idp.{options.Region}.amazonaws.com";
            var config = new AmazonCognitoIdentityProviderConfig
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region),
                ServiceURL = serviceUrl,
            };

            return new AmazonCognitoIdentityProviderClient(config);
        });
        return services;
    }
}
