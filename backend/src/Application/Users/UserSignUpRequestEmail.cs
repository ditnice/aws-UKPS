using UKPS.Api.Application.InternalServices.Communication;

namespace UKPS.Api.Application.Users;

internal class UserSignUpRequestEmail : IEmail
{
    public string Subject => "UKPS Sign Up Request";
    public required Uri Link { get; init; }
    public string HelpdeskEmail { get; init; } = "**Placeholder**";

    public string GetHtmlContent()
    {
        var content = $"""
<p>Hello,</p>
<p>
    You have been invited to register for UK PharmaScan by your organisation's champion user.
</p>
<p>
    To access UK PharmaScan you now need to activate your account.
</p>
<p>
    To activate your account please click on the following link:
</p>
<p>
    <a href="{Link}">Activate your account</a>
</p>
<p>
    Should you experience any problems activating your account then please contact the UK PharmaScan helpdesk by emailing <a href="mailto:{HelpdeskEmail}">{HelpdeskEmail}</a>.
</p>
""";
        return content;
    }
}
