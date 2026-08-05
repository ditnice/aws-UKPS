using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using UKPS.Api.Application.Authentication;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.InternalServices.Communication;
using UKPS.Api.Application.InternalServices.Hosting;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Application.InternalServices.Temporal;
using UKPS.Api.Application.Organisations;
using UKPS.Api.Application.Users;

namespace UKPS.Api.Application;

internal static class DependencyInjectionManager
{
    public static IServiceCollection AddUkpsServices(this IServiceCollection services)
    {
        services.TryAddScoped<IIdentityService, CognitoIdentityService>();

        // TODO URP 394: Add implementation for ISetupLinkCreator
        services.TryAddScoped(static _ => Substitute.For<ISetupLinkCreator>());

        // TODO URP 405: Implement the IEmailService
        services.TryAddScoped(static _ => Substitute.For<IEmailService>());

        services.TryAddScoped<ILoginService, LoginService>();
        services.TryAddScoped<IDateTimeProvider, SystemDateTimeProvider>();
        services.TryAddScoped<IOrganisationAuthoriser, OrganisationAuthoriser>();
        services.TryAddScoped<IOrganisationService, OrganisationService>();
        services.TryAddScoped<IOrganisationMembershipService, OrganisationMembershipService>();
        services.TryAddScoped<IUserService, UserService>();
        services.AddAuthenticationServices();
        services.TryAddScoped<IUserAdministrationService, UserAdministrationService>();
        services.TryAddScoped<IIdentityAdministrationService, IdentityAdministrationService>();

        return services;
    }
}
