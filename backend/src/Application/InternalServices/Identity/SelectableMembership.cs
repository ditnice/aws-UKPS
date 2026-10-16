using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.InternalServices.Identity;

/// <summary>
/// An organisation the current user can select, with their role in it.
/// </summary>
/// <param name="OrganisationId">The identifier of the organisation.</param>
/// <param name="UserRole">The user's role in the organisation.</param>
public sealed record SelectableMembership(int OrganisationId, UserRole UserRole);
