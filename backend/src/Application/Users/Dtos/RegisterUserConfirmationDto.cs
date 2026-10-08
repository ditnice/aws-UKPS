namespace UKPS.Api.Application.Users.Dtos;

/// <summary>
/// Represents confirmation of a submitted membership request.
/// </summary>
public sealed record RegisterUserConfirmationDto
{
    /// <summary>
    /// Gets the public identifier of the membership request, not a user ID or setup token.
    /// </summary>
    public required Guid RequestGuid { get; init; }

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
