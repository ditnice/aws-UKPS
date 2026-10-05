using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UKPS.Api.WebApi.InternalServices.Identity;

namespace UKPS.Api.WebApi.Authorisation;

/// <summary>
/// Defines the authorisation policies used by the UKPS API.
/// </summary>
internal static class AuthorisationPolicies
{
    /// <summary>
    /// A policy that only requires the user to be authenticated, without an organisation having
    /// been resolved. Used by the endpoints that allow the user to select their organisation.
    /// </summary>
    public const string OrganisationSelection = nameof(OrganisationSelection);

    /// <summary>
    /// Registers the UKPS authorisation policies. The default policy requires an organisation to
    /// have been resolved for the authenticated user.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddUkpsAuthorisation(this IServiceCollection services)
    {
        // Replace rather than TryAdd, as AddControllers registers the default handler.
        services.Replace(
            ServiceDescriptor.Singleton<
                IAuthorizationMiddlewareResultHandler,
                OrganisationSelectionAuthorisationResultHandler
            >()
        );

        return services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim(UkpsClaimTypes.OrganisationId)
                .Build();

            options.AddPolicy(
                OrganisationSelection,
                policy => policy.RequireAuthenticatedUser().RequireClaim(UkpsClaimTypes.Username)
            );
        });
    }
}
