namespace UKPS.Api.Application.Authentication.Dtos;

/// <summary>
/// Represents the outcome of a successful setup token resend.
/// </summary>
public record ResendSetupTokenResponse
{
    /// <summary>
    /// Gets the correlation id for the newly issued setup token, to be supplied
    /// on any subsequent resend request from the same tab instead of the
    /// (now stale) setup token.
    /// </summary>
    public required Guid CorrelationId { get; init; }
}
