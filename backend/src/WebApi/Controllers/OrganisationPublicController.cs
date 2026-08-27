using Microsoft.AspNetCore.Mvc;
using UKPS.Api.Application.Organisations;
using UKPS.Api.Application.Organisations.Dtos;

namespace UKPS.Api.WebApi.Controllers;

/// <summary>
/// Provides endpoints for retrieving and managing organisations and their memberships.
/// </summary>
/// <param name="organisationService">
/// Service used to retrieve and update organisation data and manage organisation memberships.
/// </param>
[ApiController]
[Route("organisations")]
public class OrganisationPublicController(IOrganisationService organisationService) : ControllerBase
{
    /// <summary>
    /// Gets the names of all organisations.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// the names of all organisations.
    /// </returns>
    [HttpGet("publicOptions")]
    public async Task<ActionResult<IReadOnlyCollection<OrganisationListDto>>> GetAllOrganisations(
        CancellationToken cancellationToken
    )
    {
        var organisationNames = await organisationService.GetAllOrganisations(cancellationToken);

        return Ok(organisationNames);
    }
}
