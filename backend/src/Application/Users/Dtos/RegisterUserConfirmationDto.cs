namespace UKPS.Api.Application.Users.Dtos;

/// <summary>
/// Represents the details of a user who has been registered.
/// </summary>
public sealed record RegisterUserConfirmationDto
{
    /// <summary>
    /// ID for the user.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the name of the user's organisation.
    /// </summary>
    public required string OrganisationName { get; init; }

    /// <summary>
    /// Gets the user's full name.
    /// </summary>
    public required string FullName { get; init; }

    /// <summary>
    /// Gets the user's work email address.
    /// </summary>
    public required string WorkEmail { get; init; }

    /// <summary>
    /// Gets the user phone number.
    /// </summary>
    public required string PhoneNumber { get; init; }
}
