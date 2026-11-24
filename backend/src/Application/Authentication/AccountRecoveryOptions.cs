using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.Application.Authentication;

internal sealed record AccountRecoveryOptions
{
    public const string SectionName = "AccountRecovery";

    /// <summary>
    /// How long after being requested a pending email change remains usable. Defaults to 2 hours.
    /// </summary>
    [Range(60, int.MaxValue)]
    public int PendingEmailChangeTimeoutSeconds { get; set; } = 2 * 60 * 60;

    public TimeSpan PendingEmailChangeTimeout =>
        TimeSpan.FromSeconds(PendingEmailChangeTimeoutSeconds);
}
