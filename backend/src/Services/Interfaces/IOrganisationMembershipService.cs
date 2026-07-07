using UKPS.Api.DTOs;
using UpdateUserRoleResult = UKPS.Api.Common.Result<
    UKPS.Api.DTOs.OrganisationMembershipDto,
    UKPS.Api.Services.Errors.OrganisationMembershipUpdateUserRoleError
>;

namespace UKPS.Api.Services.Interfaces;

public interface IOrganisationMembershipService
{
    Task<UpdateUserRoleResult> UpdateUserRole(
        int organisationId,
        int membershipId,
        UpdateOrgMembershipUserRoleCommandDto command,
        CancellationToken cancellationToken
    );
}
