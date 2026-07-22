namespace UKPS.Api.Services.Errors;

/// <summary>
/// Represents an error that can occur when creating an organisation.
/// </summary>
public abstract record CreateOrganisationError
{
    /// <summary>
    /// Prevents external inheritance of the <see cref="CreateOrganisationError"/> record.
    /// </summary>
    private protected CreateOrganisationError() { }

    /// <summary>
    /// Represents an error indicating that an organisation with the specified name already exists.
    /// </summary>
    public sealed record OrganisationNameConflict() : CreateOrganisationError;

    /// <summary>
    /// Represents an error indicating that one or more required fields were not provided.
    /// </summary>
    public sealed record MissingFields() : CreateOrganisationError;
}
