using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.WebApi;

internal record WebHostingOptions
{
    public const string SectionName = "WebHosting";

    [Required]
    public required Uri FrontendUri { get; init; }
}
