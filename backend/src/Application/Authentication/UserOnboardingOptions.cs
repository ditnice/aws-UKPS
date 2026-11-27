using System.ComponentModel.DataAnnotations;
using UKPS.Api.WebApi.Configuration;
using UKPS.Api.WebApi.Validators;

namespace UKPS.Api.Application.Authentication;

internal sealed record UserOnboardingOptions : IAppOptions
{
    public static string SectionName => "UserOnboarding";

    /// <summary>
    /// The amount of time it takes before the setup token expires. Defaults to 30 minutes.
    /// </summary>
    [Range(60, int.MaxValue)]
    public int SetupTokenExpiryTimeSeconds { get; set; } = 30 * 60;

    [RequiredConfiguration]
    [Range(1, int.MaxValue)]
    public int MaxSmsSetupAttempts { get; set; } = 5;

    [RequiredConfiguration]
    [Range(1, int.MaxValue)]
    public int SmsSetupCooldownSeconds { get; set; } = 30;
}
