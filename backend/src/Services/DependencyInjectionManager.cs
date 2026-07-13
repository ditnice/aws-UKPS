using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UKPS.Api.Services.Interfaces;

namespace UKPS.Api.Services;

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
