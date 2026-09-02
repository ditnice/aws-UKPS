using Microsoft.Extensions.Options;
using UKPS.Api.Application.Authentication;
using UKPS.Api.Application.InternalServices.Hosting;

namespace UKPS.Api.WebApi.InternalServices.Hosting;

internal class SetupLinkCreator : ISetupLinkCreator
{
    private readonly UserOnboardingOptions _configuration;

    public SetupLinkCreator(IOptions<UserOnboardingOptions> configuration)
    {
        _configuration = configuration.Value;
    }

    public Uri GetSetupLink(Guid setupToken)
    {
        return new Uri(_configuration.SetupLink, $"/auth/sign-up/initiate?setupToken={setupToken}");
    }
}
