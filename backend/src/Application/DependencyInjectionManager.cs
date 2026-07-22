using Microsoft.Extensions.DependencyInjection.Extensions;
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
        // TODO URP 394: Add implementation for IWebIdentityAdministrationService
        services.TryAddScoped<IWebIdentityAdministrationService>(static _ =>
            throw new NotImplementedException()
        );

        // TODO URP 394: Add implementation for ISetupLinkCreator
        services.TryAddScoped<ISetupLinkCreator>(static _ => throw new NotImplementedException());

        // TODO URP 405: Implement the IEmailService
        services.TryAddScoped<IEmailService>(static _ => throw new NotImplementedException());

        services.TryAddScoped<IDateTimeProvider, SystemDateTimeProvider>();
        services.TryAddScoped<IOrganisationAuthoriser, OrganisationAuthoriser>();
        services.TryAddScoped<IOrganisationService, OrganisationService>();
        services.TryAddScoped<IOrganisationMembershipService, OrganisationMembershipService>();
        services.TryAddScoped<IUserService, UserService>();
        services.TryAddScoped<IUserAdministrationService, UserAdministrationService>();

        return services;
    }
}
