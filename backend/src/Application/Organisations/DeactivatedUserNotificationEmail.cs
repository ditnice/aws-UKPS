using UKPS.Api.Application.InternalServices.Communication;

namespace UKPS.Api.Application.Organisations;

internal class DeactivatedUserNotificationEmail : IEmail
{
    public string Subject => "Your membership has been deactivated.";

    public string GetHtmlContent()
    {
        return "<p>Your membership has been deactivated.</p>";
    }
}
