using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UKPS.Api.Application.Users;
using UKPS.Api.WebApi.Authorisation;

namespace UKPS.Api.WebApi.Controllers;

/// <summary>
/// Provides endpoints for champion contacts associated with the current user.
/// </summary>
/// <param name="userService">The user service.</param>
[Authorize(Policy = AuthorisationPolicies.AllowDeactivatedMembership)]
[ApiController]
[Route("users/me")]
public class CurrentUserOrganisationChampionController(IUserService userService) : ControllerBase
{
    /// <summary>
    /// Gets the champion contacts for the current organisation.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The champion work email addresses.</returns>
    /// <response code="200">The champion contacts were returned.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not authorised to view champion contacts.</response>
    [HttpGet("organisations/current/champions", Name = nameof(GetCurrentOrganisationChampions))]
    [ProducesResponseType<IReadOnlyCollection<string>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyCollection<string>>> GetCurrentOrganisationChampions(
        CancellationToken cancellationToken
    )
    {
        var result = await userService.GetCurrentOrganisationChampionEmails(cancellationToken);

        return result.Match<ActionResult<IReadOnlyCollection<string>>>(
            emails => Ok(emails),
            error =>
                error.Match(notAllowed: _ =>
                    Problem(
                        statusCode: StatusCodes.Status403Forbidden,
                        title: "Forbidden",
                        detail: "You are not authorised to view champion contacts."
                    )
                )
        );
    }
}
