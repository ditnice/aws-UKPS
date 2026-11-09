using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.Application;

internal record SecurityOptions
{
    public const string SectionName = "Security";

    [Range(1, int.MaxValue)]
    public required int MaximumSecondsSinceLastAuthenticationToChangeEmail { get; init; } = 300;

    public TimeSpan MaximumTimeSinceLastAuthenticationToChangeEmail =>
        TimeSpan.FromSeconds(MaximumSecondsSinceLastAuthenticationToChangeEmail);
}
