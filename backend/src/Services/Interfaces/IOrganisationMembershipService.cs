using UKPS.Api.DTOs;
using DeactivateUserResult = UKPS.Api.Common.Result<
    UKPS.Api.DTOs.OrganisationMembershipDto,
    UKPS.Api.Services.Errors.OrganisationMembershipDeactivateUserError
>;
using UpdateUserRoleResult = UKPS.Api.Common.Result<
    UKPS.Api.DTOs.OrganisationMembershipDto,
    UKPS.Api.Services.Errors.OrganisationMembershipUpdateUserRoleError
>;

namespace UKPS.Api.Services.Interfaces;

/// <summary>
/// Defines the contract for managing organisation memberships.
/// </summary>
public interface IOrganisationMembershipService
{
    /// <summary>
    /// Updates the role of a user within an organisation membership.
    /// </summary>
    /// <param name="organisationId">The unique identifier of the organisation.</param>
    /// <param name="membershipId">The unique identifier of the membership.</param>
    /// <param name="command">The command containing the details for updating the user role.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the result of the update operation.</returns>
    Task<UpdateUserRoleResult> UpdateUserRole(
        int organisationId,
        int membershipId,
        UpdateOrgMembershipUserRoleCommandDto command,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Deactivates a membership within an organisation.
    /// </summary>
    /// <param name="organisationId">The unique identifier of the organisation.</param>
    /// <param name="membershipId">The unique identifier of the membership.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the result of the deactivation operation.</returns>
    Task<DeactivateUserResult> DeactivateMembership(
        int organisationId,
        int membershipId,
        CancellationToken cancellationToken
    );
}
