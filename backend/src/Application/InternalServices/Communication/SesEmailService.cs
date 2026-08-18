using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Microsoft.Extensions.Options;

namespace UKPS.Api.Application.InternalServices.Communication;

internal sealed partial class SesEmailService : IEmailService
{
    private readonly IAmazonSimpleEmailServiceV2 _ses;
    private readonly EmailConfiguration _configuration;
    private readonly ILogger<SesEmailService> _logger;

    public SesEmailService(
        IAmazonSimpleEmailServiceV2 ses,
        IOptions<EmailConfiguration> configuration,
        ILogger<SesEmailService> logger
    )
    {
        _ses = ses;
        _logger = logger;
        _configuration = configuration.Value;
    }

    public async Task SendEmail(string recipient, IEmail email, CancellationToken cancellationToken)
    {
        LogEmailProcessStart(recipient, email.Subject);

        var request = new SendEmailRequest
        {
            FromEmailAddress = _configuration.FromAddress,
            Destination = new Destination { ToAddresses = [recipient] },
            Content = new EmailContent
            {
                Simple = new Message
                {
                    Subject = new Content { Data = email.Subject, Charset = "UTF-8" },
                    Body = new Body
                    {
                        Html = new Content
                        {
                            Data = WrapHtml(email.GetHtmlContent()),
                            Charset = "UTF-8",
                        },
                    },
                },
            },
        };

        try
        {
            var response = await _ses.SendEmailAsync(request, cancellationToken);
            LogSuccessfulEmailSent(recipient, response.MessageId);
        }
        catch (Exception ex)
        {
            LogEmailSendError(recipient, email.Subject, ex);
            throw;
        }
    }

    private static string WrapHtml(string v)
    {
        return $"""<!DOCTYPE html><html lang="en"><body>{v}</body></html>""";
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Sending email to {Recipient} with subject {Subject}"
    )]
    private partial void LogEmailProcessStart(string recipient, string subject);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Email successfully sent to {Recipient}. Message ID: {MessageId}"
    )]
    private partial void LogSuccessfulEmailSent(string recipient, string messageId);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Failed to send email to {Recipient} with subject {Subject}"
    )]
    private partial void LogEmailSendError(string recipient, string subject, Exception ex);
}
