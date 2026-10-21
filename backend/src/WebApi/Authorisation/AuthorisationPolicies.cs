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
    /// A policy for the current-organisation champion contacts endpoint only. Allows users with a
    /// resolved organisation, and users whose membership has been deactivated. Must not be used
    /// by any other endpoint, as it admits deactivated users.
    /// </summary>
    public const string CurrentOrganisationChampionContacts = nameof(
        CurrentOrganisationChampionContacts
    );

    /// <summary>
    /// Registers the UKPS authorisation policies. The default policy requires an organisation to
    /// have been resolved for the authenticated user. Users whose membership has been deactivated are excluded.
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
                policy =>
                    policy
                        .RequireAuthenticatedUser()
                        .RequireClaim(UkpsClaimTypes.Username)
                        .RequireAssertion(context =>
                            !context.User.HasClaim(
                                UkpsClaimTypes.OrganisationSelectionFailure,
                                AuthenticationFailCode.MembershipDeactivated.ToString()
                            )
                        )
            );

            options.AddPolicy(
                CurrentOrganisationChampionContacts,
                policy =>
                    policy
                        .RequireAuthenticatedUser()
                        .RequireClaim(UkpsClaimTypes.Username)
                        .RequireAssertion(context =>
                            context.User.FindFirst(UkpsClaimTypes.OrganisationId) is not null
                            || context.User.HasClaim(
                                UkpsClaimTypes.OrganisationSelectionFailure,
                                AuthenticationFailCode.MembershipDeactivated.ToString()
                            )
                        )
            );
        });
    }
}
