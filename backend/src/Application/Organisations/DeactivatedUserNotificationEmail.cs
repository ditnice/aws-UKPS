using UKPS.Api.Application.InternalServices.Communication;

namespace UKPS.Api.Application.Organisations;

internal class DeactivatedUserNotificationEmail : IEmail
{
    public string Subject => "UKPS Membership Deactivated";
    public required string OrganisationName { get; init; }

    public string GetHtmlContent()
    {
        var content = $"""
<p>Hello,</p>
<p>
    You're membership account with {OrganisationName} has been deactivated.
</p>
""";
        return content;
    }
}
