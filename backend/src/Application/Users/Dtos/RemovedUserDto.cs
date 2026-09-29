namespace UKPS.Api.Application.Users.Dtos;

/// <summary>
/// The result of removing a user.
/// </summary>
public sealed record RemovedUserDto
{
    /// <summary>
    /// The name the removed user now appears under, for example in a record's timeline.
    /// </summary>
    public required string DisplayName { get; init; }
}
