namespace UKPS.Api.Application.InternalServices.Authorisation;

/// <summary>
/// Thrown when the current user requests an operation on an organisation they are a member of,
/// but that organisation is not their selected organisation.
/// </summary>
public class SelectedOrganisationMismatchException : Exception
{
    /// <inheritdoc cref="Exception()" />
    public SelectedOrganisationMismatchException() { }

    /// <inheritdoc cref="Exception(string)" />
    public SelectedOrganisationMismatchException(string? message)
        : base(message) { }

    /// <inheritdoc cref="Exception(string, Exception)" />
    public SelectedOrganisationMismatchException(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initialises a new instance for the organisation the operation was requested on.
    /// </summary>
    /// <param name="organisationId">The organisation the operation was requested on.</param>
    public SelectedOrganisationMismatchException(int organisationId)
        : base($"Organisation {organisationId} is not the current user's selected organisation.")
    {
        OrganisationId = organisationId;
    }

    /// <summary>
    /// Gets the organisation the operation was requested on.
    /// </summary>
    public int? OrganisationId { get; }
}
