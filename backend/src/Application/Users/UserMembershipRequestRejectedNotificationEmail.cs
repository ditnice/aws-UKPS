using UKPS.Api.Application.InternalServices.Communication;

namespace UKPS.Api.Application.Users;

internal class UserMembershipRequestRejectedNotificationEmail : IEmail
{
    public string Subject => "UKPS Membership Request Rejected";

    public string GetHtmlContent(EmailContextData contextData)
    {
        var content = $"""
<p>Hello,</p>

<p>
    Your registration request has been rejected by your organisation's champion user.
</p>

<p>
    If you have questions, contact your champion user or the UK PharmaScan helpdesk at {contextData.HelpDeskEmailLink}.
</p>

<p>
    Kind regards,
</p>

<p>
    UKPS team
</p>
""";
        return content;
    }
}
