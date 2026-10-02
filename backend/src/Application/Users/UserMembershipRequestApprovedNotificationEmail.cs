using UKPS.Api.Application.InternalServices.Communication;

namespace UKPS.Api.Application.Users;

internal class UserMembershipRequestApprovedNotificationEmail : IEmail
{
    public string Subject => "UKPS Membership Request Approved";

    public required Uri Link { get; init; }

    public string GetHtmlContent(EmailContextData contextData)
    {
        var content = $"""
<p>Hello,</p>

<p>
    Your registration request has been approved by your organisation's champion user.
</p>

<p>
    To access UK PharmaScan, you need to activate your account.
</p>

<p>
    To activate your account, use the following link:
</p>

<p>
    <a href="{Link}">Activate your UK PharmaScan account</a>
</p>

<p>
    If you have problems activating your account, contact the UK PharmaScan helpdesk at {contextData.HelpDeskEmailLink}.
</p>

<p>
    Kind regards,<br>
    UKPS team
</p>
""";
        return content;
    }
}
