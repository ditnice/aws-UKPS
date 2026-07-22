using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Tests.Utilities.Harnesses;

internal static class AuthorisationTestConstants
{
    public static CurrentUser DefaultCurrentUser { get; } =
        new CurrentUser { OrganisationId = 1, UserRole = UserRole.Super };
}
