using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.WebApi;

/// <summary>
/// Represents configuration settings describing where the application is hosted.
/// </summary>
internal sealed record HostingOptions
{
    /// <summary>
    /// Gets the configuration section name used to bind hosting settings.
    /// </summary>
    public const string SectionName = "Hosting";

    /// <summary>
    /// Gets the public base URI of the frontend, used to build absolute links sent to users
    /// (for example, account setup and email verification links).
    /// </summary>
    [Required]
    public required Uri HostUri { get; init; }
}
