using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UKPS.Api.Application.Organisations.Dtos;
using UKPS.Api.Application.Users;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.WebApi.Authorisation;

namespace UKPS.Api.WebApi.Controllers;

/// <summary>
/// Provides endpoints allowing the current user to view and select the organisation they are
/// managing. These endpoints are accessible before an organisation has been selected.
/// </summary>
/// <param name="userService">Service used to retrieve and update user data.</param>
[Authorize(Policy = AuthorisationPolicies.OrganisationSelection)]
[ApiController]
[Route("users/me")]
public class CurrentUserOrganisationController(IUserService userService) : ControllerBase
{
    /// <summary>
    /// Retrieves the organisations the current user is permitted to manage.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the request.
    /// </param>
    /// <returns>
    /// A collection of <see cref="OrganisationListDto"/> objects representing the organisations
    /// the current user can select.
    /// </returns>
    /// <response code="200">
    /// The organisations were returned successfully.
    /// </response>
    [ProducesResponseType<IReadOnlyCollection<OrganisationListDto>>(StatusCodes.Status200OK)]
    [HttpGet("organisations", Name = nameof(GetCurrentUserOrganisations))]
    public async Task<
        ActionResult<IReadOnlyCollection<OrganisationListDto>>
    > GetCurrentUserOrganisations(CancellationToken cancellationToken)
    {
        IReadOnlyCollection<OrganisationListDto> organisations =
            await userService.GetCurrentUserOrganisations(cancellationToken);
        return Ok(organisations);
    }

    /// <summary>
    /// Updates the current user's selected organisation.
    /// </summary>
    /// <param name="command">The organisation update command.</param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the request.
    /// </param>
    /// <response code="200">
    /// The current organisation was successfully updated.
    /// </response>
    /// <response code="400">
    /// The provided organisation is invalid.
    /// </response>
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpPatch("current-organisation", Name = nameof(UpdateCurrentOrganisation))]
    public async Task<ActionResult> UpdateCurrentOrganisation(
        UpdateCurrentOrganisationCommand command,
        CancellationToken cancellationToken
    )
    {
        var result = await userService.UpdateCurrentOrganisation(command, cancellationToken);
        return result.Match(
            Ok,
            err =>
                err.Match<ActionResult>(providedOrganisationWasNotValid: _ =>
                    Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Bad Request",
                        detail: "The provided organisation is invalid."
                    )
                )
        );
    }
}
