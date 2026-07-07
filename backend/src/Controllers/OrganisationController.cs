using Microsoft.AspNetCore.Mvc;
using UKPS.Api.DTOs;
using UKPS.Api.Services.Interfaces;

namespace UKPS.Api.Controllers;

[ApiController]
[Route("organisations")]
public class OrganisationController(IOrganisationService organisationService) : ControllerBase
{
    [HttpGet("{id:int}")]
    [ProducesResponseType<OrganisationDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganisationDetailsDto>> GetOrganisationById(int id)
    {
        OrganisationDetailsDto? organisation = await organisationService.GetOrganisationById(id);

        return (organisation is null) ? NotFound() : Ok(organisation);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<OrganisationDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganisationDetailsDto>> UpdateOrganisationDetails(
        int id,
        UpdateOrganisationDetailsDto organisationDetails
    )
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        OrganisationDetailsDto? organisation = await organisationService.UpdateOrganisationDetails(
            id,
            organisationDetails
        );

        return (organisation is null) ? NotFound() : Ok(organisation);
    }

    [HttpPatch("{organisationId:int}/memberships/{membershipId}")]
    [ProducesResponseType<OrganisationMembershipDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrganisationMembershipDto>> UpdateUserRole(
        int organisationId,
        int membershipId,
        UpdateOrgMembershipUserRoleCommandDto command,
        CancellationToken cancellationToken
    )
    {
        OrganisationMembershipDto? result = await organisationService.Memberships.UpdateUserRole(
            organisationId,
            membershipId,
            command,
            cancellationToken
        );
        return result is null ? NotFound() : Ok(result);
    }
}
