namespace UKPS.Api.Application.Authentication;

internal sealed record UserOnboardingConfiguration
{
    public const string SectionName = "UserOnboarding";

    /// <summary>
    /// The amount of time it takes before the setup token expires. Defaults to 15 minutes.
    /// </summary>
    public int SetupTokenExpiryTimeSeconds { get; set; } = 15 * 60;
}
