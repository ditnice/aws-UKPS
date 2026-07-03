using Microsoft.AspNetCore.Mvc;
using UKPS.Api.DTOs;
using UKPS.Api.Services.Interfaces;
using UKPS.Api.Services.Results;

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

    [HttpPost("{id:int}/users/{userId:int}/deactivate")]
    [ProducesResponseType<UserListItemDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserListItemDto>> DeactivateUser(int id, int userId)
    {
        Result<UserListItemDto> result = await organisationService.DeactivateUser(id, userId);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return result.Error.Type switch
        {
            ErrorType.NotFound => NotFound(result.Error.Description),
            ErrorType.Validation => BadRequest(result.Error.Description),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.Error.Description),
        };
    }
}
