namespace UKPS.Api.Application.Authentication.Dtos;

/// <summary>
/// Represents the command used to request that a new setup link be sent for an
/// expired setup token.
/// </summary>
public record ResendSetupTokenCommand
{
    /// <summary>
    /// Gets the expired setup token to reissue. Supplied on the first resend
    /// request for a tab, when only the token embedded in the page is known.
    /// Exactly one of SetupToken or CorrelationId must be supplied.
    /// </summary>
    public Guid? SetupToken { get; init; }

    /// <summary>
    /// Gets the correlation id returned by a previous resend, used to identify
    /// the record on a subsequent resend from the same tab without needing a
    /// currently-valid setup token. Exactly one of SetupToken or CorrelationId
    /// must be supplied.
    /// </summary>
    public Guid? CorrelationId { get; init; }
}
