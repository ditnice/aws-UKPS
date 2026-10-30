using Microsoft.Extensions.Options;
using UKPS.Api.Application.InternalServices.Hosting;

namespace UKPS.Api.WebApi.InternalServices.Hosting;

internal class PasswordResetLinkCreator : IPasswordResetLinkCreator
{
    private readonly WebHostingOptions _options;

    public PasswordResetLinkCreator(IOptions<WebHostingOptions> options)
    {
        _options = options.Value;
    }

    public Uri GetPasswordResetLink(string token)
    {
        return new Uri(_options.FrontendUri, $"/auth/password-reset?token={token}");
    }
}
