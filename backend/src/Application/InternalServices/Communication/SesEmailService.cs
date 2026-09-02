using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Microsoft.Extensions.Options;
using UKPS.Api.Persistence.Entities.Identity;

namespace UKPS.Api.Application.InternalServices.Communication;

internal sealed partial class SesEmailService : IEmailService
{
    private readonly IAmazonSimpleEmailServiceV2 _ses;
    private readonly EmailOptions _configuration;
    private readonly ILogger<SesEmailService> _logger;

    public SesEmailService(
        IAmazonSimpleEmailServiceV2 ses,
        IOptions<EmailOptions> configuration,
        ILogger<SesEmailService> logger
    )
    {
        _ses = ses;
        _logger = logger;
        _configuration = configuration.Value;
    }

    public async Task SendEmail(SendEmailCommand command, CancellationToken cancellationToken)
    {
        LogEmailProcessStart(command.CognitoUsername, command.Email.Subject);

        var request = new SendEmailRequest
        {
            FromEmailAddress = _configuration.FromAddress,
            Destination = new Destination { ToAddresses = [command.RecipientAddress] },
            Content = new EmailContent
            {
                Simple = new Message
                {
                    Subject = new Content { Data = command.Email.Subject, Charset = "UTF-8" },
                    Body = new Body
                    {
                        Html = new Content
                        {
                            Data = WrapHtml(command.Email.GetHtmlContent()),
                            Charset = "UTF-8",
                        },
                    },
                },
            },
        };

        try
        {
            SendEmailResponse response = await _ses.SendEmailAsync(request, cancellationToken);
            LogSuccessfulEmailSent(command.CognitoUsername, response.MessageId);
        }
        catch (Exception ex)
        {
            LogEmailSendError(command.CognitoUsername, command.Email.Subject, ex);
            throw;
        }
    }

    private static string WrapHtml(string v)
    {
        return $"""<!DOCTYPE html><html lang="en"><body>{v}</body></html>""";
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Sending email to {Username} with subject {Subject}"
    )]
    private partial void LogEmailProcessStart(CognitoUsername username, string subject);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Email successfully sent to {Username}. Message ID: {MessageId}"
    )]
    private partial void LogSuccessfulEmailSent(CognitoUsername username, string messageId);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Failed to send email to {Username} with subject {Subject}"
    )]
    private partial void LogEmailSendError(CognitoUsername username, string subject, Exception ex);
}
