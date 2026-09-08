using UKPS.Api.Application.InternalServices.Communication;

namespace UKPS.Api.Application.Organisations;

internal class ReactivatedUserNotificationEmail : IEmail
{
    public string Subject => "UKPS Membership Reactivated";
    public required string OrganisationName { get; init; }

    public string GetHtmlContent()
    {
        var content = $"""
<p>
  Hello,<br>
  This email confirms that your UK PharmaScan account for {OrganisationName} has been reactivated.
</p>

<p>
  If you have questions or want to restore access to UK PharmaScan, contact your organisation's champion user.
</p>

<p>
  Kind regards,<br>
  UKPS team
</p>
""";
        return content;
    }
}
