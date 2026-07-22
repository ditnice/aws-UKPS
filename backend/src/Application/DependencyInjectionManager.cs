using Microsoft.Extensions.DependencyInjection.Extensions;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.Organisations;
using UKPS.Api.Application.Users;

namespace UKPS.Api.Application;

internal static class DependencyInjectionManager
{
    public static IServiceCollection AddUkpsServices(this IServiceCollection services)
    {
        services.TryAddScoped<IOrganisationAuthoriser, OrganisationAuthoriser>();
        services.TryAddScoped<IOrganisationService, OrganisationService>();
        services.TryAddScoped<IOrganisationMembershipService, OrganisationMembershipService>();
        services.TryAddScoped<IUserService, UserService>();

        return services;
    }
}
