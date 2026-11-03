using UKPS.Api.Persistence.Entities.Identity;

namespace UKPS.Api.WebApi.InternalServices.Authentication;

/// <summary>
/// A user that development authentication can act as. Seeding creates one per role.
/// </summary>
internal sealed record DevAuthenticationUser(CognitoUsername CognitoUsername, string Email);
