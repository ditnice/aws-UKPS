using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.Application.Authentication;

internal sealed record UserOnboardingOptions
{
    public const string SectionName = "UserOnboarding";

    /// <summary>
    /// The amount of time it takes before the setup token expires. Defaults to 30 minutes.
    /// </summary>
    [Range(60, int.MaxValue)]
    public int SetupTokenExpiryTimeSeconds { get; set; } = 30 * 60;

    [Required]
    public required Uri SetupLink { get; init; }

    /// <summary>
    /// The key used to encrypt short-lived onboarding passwords. Must be supplied via configuration.
    /// </summary>
    [Required]
    [MinLength(16)]
    public required string PasswordStorageKey { get; set; }

    [Required]
    public int StoredPasswordExpirySeconds { get; set; } = 300;

    [Required]
    [Range(1, int.MaxValue)]
    public int MaxSmsSetupAttempts { get; set; } = 5;

    [Required]
    [Range(1, int.MaxValue)]
    public int SmsSetupCooldownSeconds { get; set; } = 30;
}
