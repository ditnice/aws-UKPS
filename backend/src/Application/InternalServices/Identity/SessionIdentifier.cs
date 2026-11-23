namespace UKPS.Api.Application.InternalServices.Identity;

/// <summary>
/// Represents an identifier used to distinguish between an authentication session
/// and an access token.
/// </summary>
public abstract record SessionIdentifier
{
    /// <summary>
    /// Represents an authentication session identifier.
    /// </summary>
    /// <param name="Value">The authentication session identifier.</param>
    public sealed record AuthenticationSession(string Value) : SessionIdentifier;

    /// <summary>
    /// Represents an access token identifier.
    /// </summary>
    /// <param name="Value">The access token value.</param>
    public sealed record AccessToken(string Value) : SessionIdentifier;

    internal T Match<T>(Func<AuthenticationSession, T> onSession, Func<AccessToken, T> onToken) =>
        this switch
        {
            AuthenticationSession s => onSession(s),
            AccessToken t => onToken(t),
            _ => throw new InvalidOperationException(),
        };
}
