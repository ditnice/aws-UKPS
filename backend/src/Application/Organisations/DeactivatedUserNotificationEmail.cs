using UKPS.Api.Application.InternalServices.Communication;

namespace UKPS.Api.Application.Organisations;

internal class DeactivatedUserNotificationEmail : IEmail
{
    public string Subject => "UKPS Membership Deactivated";
    public required string OrganisationName { get; init; }

    public string GetHtmlContent()
    {
        // TODO URP-534: Write the deactivation notification email.
        var content = $"""
**PLACEHOLDER**
""";
        return content;
    }
}
