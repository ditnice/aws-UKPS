namespace UKPS.Api.Application.Users.Dtos;

/// <summary>
/// Represents a requested change to a user's work email address that is awaiting completion.
/// </summary>
public sealed record PendingWorkEmailChangeDto
{
    /// <summary>
    /// Gets the new work email address that has been requested.
    /// </summary>
    public required string Value { get; init; }

    /// <summary>
    /// Gets whether the new work email address has been verified by email.
    /// </summary>
    /// <remarks>
    /// Email verification is not yet implemented.
    /// </remarks>
    public required bool VerifiedByEmail { get; init; }
}
