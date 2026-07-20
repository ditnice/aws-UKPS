using UKPS.Api.Enums;
using UKPS.Api.Services.Interfaces;

namespace UKPS.Api.Tests.Utilities.Harnesses;

internal static class AuthorisationTestConstants
{
    public static CurrentUser DefaultCurrentUser { get; } =
        new CurrentUser { OrganisationId = 1, UserRole = UserRole.Super };
}
