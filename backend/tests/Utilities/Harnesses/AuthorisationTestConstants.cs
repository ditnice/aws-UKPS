using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Tests.Utilities.Harnesses;

internal static class AuthorisationTestConstants
{
    public static CurrentUser DefaultCurrentUser { get; } =
        new CurrentUser
        {
            OrganisationId = 1,
            UserRole = UserRole.Super,
            Email = "exampleuser@email.com",
            CognitoUsername = CognitoUsername.Parse("24a97ae0-d7d2-4c6b-88de-d835b13e8038"),
        };
}
