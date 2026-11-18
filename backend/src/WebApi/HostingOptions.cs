using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.WebApi;

internal sealed record HostingOptions
{
    public const string SectionName = "Hosting";

    [Required]
    public required Uri HostUri { get; init; }
}
