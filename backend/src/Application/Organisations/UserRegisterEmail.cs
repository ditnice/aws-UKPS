using System.Net;
using UKPS.Api.Application.InternalServices.Communication;

namespace UKPS.Api.Application.Organisations;

internal class UserRegisterEmail : IEmail
{
    public string Subject => "UKPS Requested Access";
    public required string OrganisationName { get; init; }

    public string GetHtmlContent(EmailContextData contextData)
    {
        var htmlEncodedOrgName = WebUtility.HtmlEncode(OrganisationName);
        var content = $"""
<p>
  Hello,<br>
  This email confirms that your UK PharmaScan account for {htmlEncodedOrgName} has been requested.
</p>

<p>
  Kind regards,<br>
  UKPS team
</p>
""";
        return content;
    }
}
