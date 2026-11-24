using UKPS.Api.Application.InternalServices.Communication;

namespace UKPS.Api.Application.Records;

internal class RecordArchivedNotificationEmail : IEmail
{
    public string Subject => "UKPS Record Archived";
    public required int RecordId { get; init; }

    public string GetHtmlContent(EmailContextData contextData)
    {
        var content = $"""
            <p>
            Hello,<br>
            This email confirms that record {RecordId} has been archived.
            </p>

            <p>
            This record can no longer be updated, but it will remain visible to horizon scanning organisations.
            </p>

            <p>
            If you have questions, contact the UK PharmaScan helpdesk at {contextData.HelpDeskEmailLink}.
            </p>

            <p>
            Kind regards,<br>
            UKPS team
            </p>
            """;
        return content;
    }
}
