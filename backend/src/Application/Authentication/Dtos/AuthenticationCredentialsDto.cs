namespace UKPS.Api.Application.Authentication.Dtos;

/// <summary>
/// Represents the authentication credentials returned after a successful login.
/// </summary>
public sealed record AuthenticationCredentialsDto
{
    /// <summary>
    /// Gets the access token issued by the identity provider.
    /// </summary>
    public required string AccessToken { get; init; }
}
