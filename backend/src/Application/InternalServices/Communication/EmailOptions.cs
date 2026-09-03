using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.Application.InternalServices.Communication;

internal record EmailOptions
{
    public const string SectionName = "Email";

    [Required]
    public required string FromAddress { get; init; }

    [Required]
    public required string QueueUrl { get; init; }

    public int QueueMaxNumberOfMessages { get; init; } = 1;
    public int QueueWaitTimeInSeconds { get; init; } = 20;
}
