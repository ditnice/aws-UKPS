using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.Application.Authentication;

internal record AuthenticationOptions
{
    public const string SectionName = "Authentication";

    [Range(1, int.MaxValue)]
    public int MaxNumberOfChangePasswordAttempts { get; init; } = 3;
}
