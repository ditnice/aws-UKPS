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

    [Required]
    [Range(1, int.MaxValue)]
    public int MaxSmsSetupAttempts { get; set; } = 5;

    [Required]
    [Range(1, int.MaxValue)]
    public int SmsSetupCooldownSeconds { get; set; } = 30;
}
