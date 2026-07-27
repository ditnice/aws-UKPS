namespace UKPS.Api.Application.Users.Errors;

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

    /// <summary>
    /// Represents an error indicating that a user with the given email already exists.
    /// </summary>
    public sealed record EmailConflict() : CreateUserError;

    /// <summary>
    /// Represents an error indicating that one or more required fields were not provided.
    /// </summary>
    public sealed record MissingFields() : CreateUserError;
}
