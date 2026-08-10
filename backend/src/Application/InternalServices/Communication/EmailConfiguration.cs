using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.Application.InternalServices.Communication;

internal record EmailConfiguration
{
    public const string SectionName = "Email";

    [Required]
    public required string FromAddress { get; init; }
}
