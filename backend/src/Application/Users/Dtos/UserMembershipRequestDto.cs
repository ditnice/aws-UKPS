namespace UKPS.Api.Application.Users.Dtos;

/// <summary>
/// Represents a request for a user's membership.
/// </summary>
public record UserMembershipRequestDto
{
    /// <summary>
    /// Gets the unique identifier of the membership request.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// The email associated with the email address.
    /// </summary>
    public required string WorkEmail { get; init; }
}
