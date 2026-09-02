using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.Application.InternalServices.Communication;

internal record EmailOptions
{
    public const string SectionName = "Email";

    [Required]
    public required string FromAddress { get; init; }

    [Required]
    public required string QueueUrl { get; init; }
}
