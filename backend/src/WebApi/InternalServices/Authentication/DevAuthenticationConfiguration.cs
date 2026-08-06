namespace UKPS.Api.WebApi.InternalServices.Authentication;

/// <summary>
/// Represents configuration settings for development authentication.
/// </summary>
public sealed record DevAuthenticationConfiguration
{
    /// <summary>
    /// Gets the configuration section name used to bind development authentication settings.
    /// </summary>
    public const string SectionName = "DevAuthentication";

    /// <summary>
    /// Gets a value indicating whether development authentication is enabled.
    /// </summary>
    public bool IsEnabled { get; init; }
}
