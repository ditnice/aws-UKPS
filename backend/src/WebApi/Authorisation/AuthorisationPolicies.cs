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
    /// A policy that allows deactivated users. Allows users with a resolved organisation, and
    /// users whose membership has been deactivated. Only use it for endpoints a deactivated user
    /// needs, such as the champion contacts shown on the deactivated page.
    /// </summary>
    public const string AllowDeactivatedMembership = nameof(AllowDeactivatedMembership);

    /// <summary>
    /// Registers the UKPS authorisation policies. The default policy requires an organisation to
    /// have been resolved for the authenticated user. Users whose membership has been deactivated
    /// are excluded.
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
                AllowDeactivatedMembership,
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
