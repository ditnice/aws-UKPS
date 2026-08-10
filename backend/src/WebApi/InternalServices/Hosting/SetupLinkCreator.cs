using UKPS.Api.Application.InternalServices.Hosting;

namespace UKPS.Api.WebApi.InternalServices.Hosting;

internal class SetupLinkCreator : ISetupLinkCreator
{
    public string GetSetupLink(Guid setupToken)
    {
        return $"placeholder?setupToken={setupToken}";
    }
}
