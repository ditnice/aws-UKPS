using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.Application;

/// <summary>
/// Configuration options for security-related behavior, bound from the
/// <see cref="SectionName"/> configuration section.
/// </summary>
public record SecurityOptions
{
    /// <summary>
    /// The name of the configuration section these options are bound from.
    /// </summary>
    public const string SectionName = "Security";

    /// <summary>
    /// Gets the maximum number of seconds that may have elapsed since the user's last
    /// authentication for them to be allowed to change their email address.
    /// </summary>
    /// <value>
    /// A positive number of seconds. The default is 900 (15 minutes).
    /// </value>
    /// <remarks>
    /// If more time than this has passed since the user last authenticated, they must
    /// re-authenticate before changing their email. Use
    /// <see cref="MaximumTimeSinceLastAuthenticationToChangeEmail"/> to get this value
    /// as a <see cref="TimeSpan"/>.
    /// </remarks>
    [Range(1, int.MaxValue)]
    public required int MaximumSecondsSinceLastAuthenticationToChangeEmail { get; init; } = 900;

    /// <summary>
    /// Gets the maximum time that may have elapsed since the user's last authentication
    /// for them to be allowed to change their email address.
    /// </summary>
    /// <value>
    /// A <see cref="TimeSpan"/> equivalent to
    /// <see cref="MaximumSecondsSinceLastAuthenticationToChangeEmail"/>.
    /// </value>
    public TimeSpan MaximumTimeSinceLastAuthenticationToChangeEmail =>
        TimeSpan.FromSeconds(MaximumSecondsSinceLastAuthenticationToChangeEmail);
}
