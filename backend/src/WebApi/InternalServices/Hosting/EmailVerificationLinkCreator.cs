using Microsoft.Extensions.Options;
using UKPS.Api.Application.InternalServices.Hosting;

namespace UKPS.Api.WebApi.InternalServices.Hosting;

internal class EmailVerificationLinkCreator : IEmailVerificationLinkCreator
{
    private readonly HostingOptions _options;

    public EmailVerificationLinkCreator(IOptions<HostingOptions> options)
    {
        _options = options.Value;
    }

    public Uri GetVerificationLink(string verificationToken)
    {
        return new Uri(
            _options.HostUri,
            $"/portal/user/me/verify-email?verificationToken={verificationToken}"
        );
    }
}
