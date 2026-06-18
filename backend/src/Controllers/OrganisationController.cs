using Microsoft.AspNetCore.Mvc;
using UKPS.Api.DTOs;
using UKPS.Api.Services;

namespace UKPS.Api.Controllers;

[ApiController]
[Route("organisations")]
public class OrganisationController(IOrganisationService organisationService) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrganisationById(int id)
    {
        OrganisationDetailsDto? organisation = await organisationService.GetOrganisationById(id).ConfigureAwait(false);

        return (organisation is null)
         ? NotFound()
         : Ok(organisation);
    }
}
