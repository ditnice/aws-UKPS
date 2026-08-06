using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.Application.Authentication;

/// <summary>
/// Represents a request to refresh an authentication token using an existing refresh token.
/// </summary>
public record RefreshAuthenticationTokenCommand
{
    /// <summary>
    /// Gets the refresh token used to obtain a new authentication token.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public required string RefreshToken { get; init; }
}
