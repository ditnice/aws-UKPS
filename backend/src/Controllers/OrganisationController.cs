using Microsoft.AspNetCore.Mvc;
using UKPS.Api.DTOs;
using UKPS.Api.Services;

namespace UKPS.Api.Controllers;

[ApiController]
[Route("organisations")]
public class OrganisationController(IOrganisationService organisationService) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        OrganisationDetailsDto? organisation = await organisationService.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);

        if (organisation is null)
            return NotFound();

        return Ok(organisation);
    }
}
