using UKPS.Api.Common;
using UKPS.Api.DTOs;
using UKPS.Api.Services.Errors;

namespace UKPS.Api.Services.Interfaces;

/// <summary>
/// Defines the contract for organisation-related operations.
/// </summary>
public interface IOrganisationService
{
    /// <summary>
    /// Gets the service responsible for managing organisation memberships.
    /// </summary>
    IOrganisationMembershipService Memberships { get; }

    /// <summary>
    /// Retrieves the details of an organisation by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the organisation.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// a <see cref="Result{TSuccess, TError}"/> object with the organisation details
    /// or an error of type <see cref="GetOrganisationByIdError"/>.
    /// </returns>
    Task<Result<OrganisationDetailsDto, GetOrganisationByIdError>> GetOrganisationById(
        int id,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Updates the details of an organisation.
    /// </summary>
    /// <param name="id">The unique identifier of the organisation to update.</param>
    /// <param name="organisationDetails">The updated organisation details.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// a <see cref="Result{TSuccess, TError}"/> object with the updated organisation details
    /// or an error of type <see cref="UpdateOrganisationDetailsError"/>.
    /// </returns>
    Task<Result<OrganisationDetailsDto, UpdateOrganisationDetailsError>> UpdateOrganisationDetails(
        int id,
        UpdateOrganisationDetailsDto organisationDetails,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Creates a new organisation.
    /// </summary>
    /// <param name="command">
    /// The details required to create the organisation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a
    /// <see cref="Result{TSuccess, TError}"/> object with the created organisation's details
    /// or an error of type <see cref="CreateOrganisationError"/>.
    /// </returns>
    Task<Result<OrganisationDetailsDto, CreateOrganisationError>> CreateOrganisation(
        CreateOrganisationDto command
    );
}
