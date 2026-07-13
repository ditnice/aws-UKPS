using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UKPS.Api.DTOs;
using UKPS.Api.Services.Errors;
using UKPS.Api.Services.Interfaces;

namespace UKPS.Api.Controllers;

[ApiController]
[Route("organisations")]
public class OrganisationController(IOrganisationService organisationService) : ControllerBase
{
    [HttpGet("{id:int}")]
    [ProducesResponseType<OrganisationDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganisationDetailsDto>> GetOrganisationById(
        int id,
        CancellationToken cancellationToken
    )
    {
        var result = await organisationService.GetOrganisationById(id, cancellationToken);

        return result.Match<ActionResult<OrganisationDetailsDto>>(
            organisation => Ok(organisation),
            error =>
                error switch
                {
                    GetOrganisationByIdError.NotFound => NotFound(),
                    _ => throw new UnreachableException(
                        "Unhandled GetOrganisationByIdError variant."
                    ),
                }
        );
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<OrganisationDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganisationDetailsDto>> UpdateOrganisationDetails(
        int id,
        UpdateOrganisationDetailsDto organisationDetails,
        CancellationToken cancellationToken
    )
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await organisationService.UpdateOrganisationDetails(
            id,
            organisationDetails,
            cancellationToken
        );

        return result.Match<ActionResult<OrganisationDetailsDto>>(
            organisation => Ok(organisation),
            error =>
                error switch
                {
                    UpdateOrganisationDetailsError.NotFound => NotFound(),
                    _ => throw new UnreachableException(
                        "Unhandled UpdateOrganisationDetailsError variant."
                    ),
                }
        );
    }

    [HttpPatch("{organisationId:int}/memberships/{membershipId}/deactivate")]
    [ProducesResponseType<OrganisationMembershipDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganisationMembershipDto>> DeactivateMembership(
        int organisationId,
        int membershipId,
        CancellationToken cancellationToken
    )
    {
        var result = await organisationService.Memberships.DeactivateMembership(
            organisationId,
            membershipId,
            cancellationToken
        );

        return result.Match<ActionResult<OrganisationMembershipDto>>(
            x => Ok(x),
            x =>
                x switch
                {
                    OrganisationMembershipDeactivateUserError.NotFound => NotFound(
                        $"Could not find a membership with organisation ID = {organisationId} and membership ID = {membershipId}."
                    ),
                    _ => throw new UnreachableException(),
                }
        );
    }

    [HttpPatch("{organisationId:int}/memberships/{membershipId}/update-role")]
    [ProducesResponseType<OrganisationMembershipDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganisationMembershipDto>> UpdateUserRole(
        int organisationId,
        int membershipId,
        UpdateOrgMembershipUserRoleCommandDto command,
        CancellationToken cancellationToken
    )
    {
        var result = await organisationService.Memberships.UpdateUserRole(
            organisationId,
            membershipId,
            command,
            cancellationToken
        );
        return result.Match<ActionResult<OrganisationMembershipDto>>(
            x => Ok(x),
            x =>
                x switch
                {
                    OrganisationMembershipUpdateUserRoleError.NotFound notFound => NotFound(
                        $"Could not find a membership with organisation ID = {notFound.OrganisationId} and membership ID = {notFound.MembershipId}."
                    ),
                    _ => throw new UnreachableException(),
                }
        );
    }
}
