namespace UKPS.Api.Application.Organisations.Errors;

/// <summary>
/// Represents the base class for errors that can occur when updating organisation details.
/// </summary>
public abstract record UpdateOrganisationDetailsError
{
    /// <summary>
    /// Represents an error that occurs when the operation is not allowed for the specified organisation.
    /// </summary>
    /// <param name="OrganisationId">The identifier of the organisation for which the operation is not allowed.</param>
    public sealed record NotAllowed(int OrganisationId) : UpdateOrganisationDetailsError;

    /// <summary>
    /// Represents an error that occurs when the specified organisation is not found.
    /// </summary>
    /// <param name="OrganisationId">The identifier of the organisation that was not found.</param>
    public sealed record NotFound(int OrganisationId) : UpdateOrganisationDetailsError;
}
