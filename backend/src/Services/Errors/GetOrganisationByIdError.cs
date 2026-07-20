namespace UKPS.Api.Services.Errors;

/// <summary>
/// Represents the base class for errors that can occur when attempting to retrieve an organisation by its ID.
/// </summary>
public abstract record GetOrganisationByIdError
{
    /// <summary>
    /// Initialises a new instance of the <see cref="GetOrganisationByIdError"/> class.
    /// </summary>
    private protected GetOrganisationByIdError() { }

    /// <summary>
    /// Represents an error that occurs when the operation is not allowed for the specified organisation ID.
    /// </summary>
    /// <param name="OrganisationId">The ID of the organisation for which the operation is not allowed.</param>
    internal sealed record NotAllowed(int OrganisationId) : GetOrganisationByIdError;

    /// <summary>
    /// Represents an error that occurs when the specified organisation ID is not found.
    /// </summary>
    /// <param name="OrganisationId">The ID of the organisation that was not found.</param>
    public sealed record NotFound(int OrganisationId) : GetOrganisationByIdError;
}
