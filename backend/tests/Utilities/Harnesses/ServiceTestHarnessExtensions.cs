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
        return serviceTestHarness.UpdateCurrentUser(x =>
            x with
            {
                CognitoUsername = user.CognitoUsername,
                UserRole = user.UserOrgMemberships!.First().UserRole,
                Email = user.WorkEmail,
            }
        );
    }
}
