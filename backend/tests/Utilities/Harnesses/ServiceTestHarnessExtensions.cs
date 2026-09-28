using UKPS.Api.Persistence.Entities.Identity;

namespace UKPS.Api.Tests.Utilities.Harnesses;

internal static class ServiceTestHarnessExtensions
{
    public static IServiceTestHarness<TService> UpdateCurrentUser<TService>(
        this IServiceTestHarness<TService> serviceTestHarness,
        User user
    )
        where TService : notnull
    {
        UserOrgMembership membership = user.UserOrgMemberships!.First();
        return serviceTestHarness.UpdateCurrentUser(x =>
            x with
            {
                CognitoUsername = user.CognitoUsername,
                OrganisationId = membership.OrganisationId,
                UserRole = membership.UserRole,
                Email = user.WorkEmail,
            }
        );
    }
}
