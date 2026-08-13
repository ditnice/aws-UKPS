using Microsoft.Extensions.Options;
using UKPS.Api.Application.Authentication;
using UKPS.Api.Application.InternalServices.Hosting;

namespace UKPS.Api.WebApi.InternalServices.Hosting;

internal class SetupLinkCreator : ISetupLinkCreator
{
    private readonly UserOnboardingConfiguration _configuration;

    public SetupLinkCreator(IOptions<UserOnboardingConfiguration> configuration)
    {
        _configuration = configuration.Value;
    }

    public Uri GetSetupLink(Guid setupToken)
    {
        return new Uri(
            _configuration.SetupLink,
            $"/portal/auth/sign-up/initiate?setupToken={setupToken}"
        );
    }
}
