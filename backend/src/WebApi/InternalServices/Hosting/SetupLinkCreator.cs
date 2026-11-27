using Microsoft.Extensions.Options;
using UKPS.Api.Application.InternalServices.Hosting;

namespace UKPS.Api.WebApi.InternalServices.Hosting;

internal class SetupLinkCreator : ISetupLinkCreator
{
    private readonly HostingOptions _configuration;

    public SetupLinkCreator(IOptions<HostingOptions> configuration)
    {
        _configuration = configuration.Value;
    }

    public Uri GetSetupLink(Guid setupToken)
    {
        return new Uri(_configuration.HostUri, $"/auth/sign-up/initiate?setupToken={setupToken}");
    }
}
