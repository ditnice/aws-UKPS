using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.Application.InternalServices.Communication;

internal record EmailOptions
{
    public const string SectionName = "Email";

    public int MaxResendSignUpLinkAttempts { get; set; } = 3;

    [Required]
    public required string FromAddress { get; init; }
}
