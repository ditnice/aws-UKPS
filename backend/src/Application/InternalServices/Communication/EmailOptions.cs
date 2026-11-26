using System.ComponentModel.DataAnnotations;
using UKPS.Api.WebApi.Configuration;
using UKPS.Api.WebApi.Validators;

namespace UKPS.Api.Application.InternalServices.Communication;

internal record EmailOptions : IAppOptions
{
    public static string SectionName => "Email";

    public int MaxResendSignUpLinkAttempts { get; set; } = 3;

    public bool ListenerEnabled { get; init; } = true;

    [EmailAddress]
    public string? TestingEmailAddress { get; init; }

    [RequiredConfiguration]
    public required string FromAddress { get; init; }

    [RequiredConfiguration]
    public required string QueueUrl { get; init; }

    [Range(1, 10)]
    public int QueueMaxNumberOfMessages { get; init; } = 1;

    [Range(0, 20)]
    public int QueueWaitTimeInSeconds { get; init; } = 20;
}
