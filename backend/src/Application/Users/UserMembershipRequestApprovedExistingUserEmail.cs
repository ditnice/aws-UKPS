using System.Net;
using UKPS.Api.Application.InternalServices.Communication;

namespace UKPS.Api.Application.Users;

internal class UserMembershipRequestApprovedExistingUserEmail : IEmail
{
    public string Subject => "UKPS Membership Request Approved";
    public required string OrganisationName { get; init; }

    public string GetHtmlContent(EmailContextData contextData)
    {
        var htmlEncodedOrgName = WebUtility.HtmlEncode(OrganisationName);
        var content = $"""
<p>Hello,</p>

<p>
  Your request to join {htmlEncodedOrgName} has been approved by the organisation's champion user.
</p>

<p>
  You now have access to {htmlEncodedOrgName}. You can sign in to UK PharmaScan as usual.
</p>

<p>
  If you have not yet activated your account, use the activation link from your earlier email.
</p>

<p>
  If you have questions, contact your champion user or the UK PharmaScan helpdesk at {contextData.HelpDeskEmailLink}.
</p>

<p>
  Kind regards,<br>
  UKPS team
</p>
""";
        return content;
    }
}
