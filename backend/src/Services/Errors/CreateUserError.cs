namespace UKPS.Api.Services.Errors;

/// <summary>
/// Represents an error that can occur when creating a user.
/// </summary>
public abstract record CreateUserError
{
    /// <summary>
    /// Prevents external inheritance of the <see cref="CreateUserError"/> record.
    /// </summary>
    private protected CreateUserError() { }

    /// <summary>
    /// Represents an error indicating that the specified organisation could not be found.
    /// </summary>
    /// <param name="OrganisationId">
    /// The identifier of the organisation that could not be found.
    /// </param>
    internal sealed record NotFound(int OrganisationId) : CreateUserError;
}
