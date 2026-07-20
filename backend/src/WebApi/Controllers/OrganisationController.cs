using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UKPS.Api.DTOs;
using UKPS.Api.Services.Errors;
using UKPS.Api.Services.Interfaces;

namespace UKPS.Api.WebApi.Controllers;

/// <summary>
/// Provides endpoints for retrieving and managing organisations and their memberships.
/// </summary>
/// <param name="organisationService">
/// Service used to retrieve and update organisation data and manage organisation memberships.
/// </param>
[ApiController]
[Route("organisations")]
public class OrganisationController(IOrganisationService organisationService) : ControllerBase
{
    /// <summary>
    /// Retrieves an organisation by its unique identifier.
    /// </summary>
    /// <param name="id">The identifier of the organisation to retrieve.</param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// An <see cref="OrganisationDetailsDto"/> containing the organisation details if found.
    /// </returns>
    /// <response code="200">
    /// The organisation was found and its details were returned.
    /// </response>
    /// <response code="404">
    /// No organisation exists with the specified identifier.
    /// </response>
    [HttpGet("{id:int}", Name = nameof(GetOrganisationById))]
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
                    GetOrganisationByIdError.NotAllowed => Forbid(),
                    _ => throw new UnreachableException(
                        "Unhandled GetOrganisationByIdError variant."
                    ),
                }
        );
    }

    /// <summary>
    /// Updates the details of an existing organisation.
    /// </summary>
    /// <param name="id">The identifier of the organisation to update.</param>
    /// <param name="organisationDetails">
    /// The updated organisation details.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// The updated <see cref="OrganisationDetailsDto"/>.
    /// </returns>
    /// <response code="200">
    /// The organisation was successfully updated.
    /// </response>
    /// <response code="400">
    /// The supplied organisation details failed validation.
    /// </response>
    /// <response code="404">
    /// No organisation exists with the specified identifier.
    /// </response>
    [HttpPut("{id:int}", Name = nameof(UpdateOrganisationDetails))]
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
                    UpdateOrganisationDetailsError.NotAllowed => Forbid(),
                    _ => throw new UnreachableException(
                        "Unhandled UpdateOrganisationDetailsError variant."
                    ),
                }
        );
    }

    /// <summary>
    /// Deactivates an organisation membership.
    /// </summary>
    /// <param name="organisationId">
    /// The identifier of the organisation that owns the membership.
    /// </param>
    /// <param name="membershipId">
    /// The identifier of the membership to deactivate.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// The updated <see cref="OrganisationMembershipDto"/> representing the deactivated membership.
    /// </returns>
    /// <response code="200">
    /// The membership was successfully deactivated.
    /// </response>
    /// <response code="404">
    /// The specified organisation membership could not be found.
    /// </response>
    [HttpPatch(
        "{organisationId:int}/memberships/{membershipId}/deactivate",
        Name = nameof(DeactivateMembership)
    )]
    [ProducesResponseType<OrganisationMembershipDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
                    OrganisationMembershipDeactivateUserError.NotAllowed => Forbid(
                        "The user is not authorised to perform this action."
                    ),
                    _ => throw new UnreachableException(),
                }
        );
    }

    /// <summary>
    /// Updates the role assigned to a user within an organisation membership.
    /// </summary>
    /// <param name="organisationId">
    /// The identifier of the organisation that owns the membership.
    /// </param>
    /// <param name="membershipId">
    /// The identifier of the membership to update.
    /// </param>
    /// <param name="command">
    /// The request containing the new role assignment details.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// The updated <see cref="OrganisationMembershipDto"/>.
    /// </returns>
    /// <response code="200">
    /// The membership role was successfully updated.
    /// </response>
    /// <response code="404">
    /// The specified organisation membership could not be found.
    /// </response>
    [HttpPatch(
        "{organisationId:int}/memberships/{membershipId}/update-role",
        Name = nameof(UpdateUserRole)
    )]
    [ProducesResponseType<OrganisationMembershipDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
                    OrganisationMembershipUpdateUserRoleError.NotAllowed => Forbid(
                        "The user is not authorised to perform this action."
                    ),
                    _ => throw new UnreachableException(),
                }
        );
    }
}
