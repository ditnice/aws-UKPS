using UKPS.Api.WebApi.Configuration;
using UKPS.Api.WebApi.Validators;

namespace UKPS.Api.WebApi;

internal sealed record HostingOptions : IAppOptions
{
    public static string SectionName => "Hosting";

    [RequiredConfiguration]
    public required Uri HostUri { get; init; }
}
