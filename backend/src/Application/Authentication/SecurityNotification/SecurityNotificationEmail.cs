using System.Globalization;
using UKPS.Api.Application.InternalServices.Communication;

namespace UKPS.Api.Application.Authentication.SecurityNotification;

internal sealed record SecurityNotificationEmail : IEmail
{
    public string Subject => $"Your {FormattedChangedValue} was changed";
    public required ChangedUserData ChangedValue { get; init; }

    private string FormattedChangedValue
    {
        get => ChangedValue.ToString().ToLower(CultureInfo.CurrentCulture);
    }

    public string GetHtmlContent(EmailContextData contextData)
    {
        return $"""
            <p>Your {FormattedChangedValue} was recently changed.</p>
            <p>If you made this change, no further action is needed.</p>
            <p>If you didn't, please contact support at {contextData.HelpDeskEmailLink}.</p>
            """;
    }
}
