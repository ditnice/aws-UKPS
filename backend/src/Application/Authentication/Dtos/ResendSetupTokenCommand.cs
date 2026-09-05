namespace UKPS.Api.Application.Authentication.Dtos;

/// <summary>
/// Represents the command used to request that a new setup link be sent for an
/// expired setup token.
/// </summary>
public record ResendSetupTokenCommand
{
    /// <summary>
    /// Gets the expired setup token to reissue.
    /// </summary>
    public required Guid SetupToken { get; init; }
}
