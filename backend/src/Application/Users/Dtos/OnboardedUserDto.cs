namespace UKPS.Api.Application.Users.Dtos;

/// <summary>
/// Represents the outcome of onboarding a new user.
/// </summary>
public sealed record OnboardedUserDto
{
    /// <summary>
    /// Gets the unique identifier of the newly onboarded user.
    /// </summary>
    public required int UserId { get; init; }
}
