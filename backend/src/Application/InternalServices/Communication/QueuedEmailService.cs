using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;

namespace UKPS.Api.Application.InternalServices.Communication;

internal sealed partial class QueuedEmailService : IEmailService
{
    private readonly IAmazonSQS _sqs;
    private readonly EmailOptions _emailOptions;
    private readonly ILogger<QueuedEmailService> _logger;

    public QueuedEmailService(
        IAmazonSQS sqs,
        IOptions<EmailOptions> emailOptions,
        ILogger<QueuedEmailService> logger
    )
    {
        _sqs = sqs;
        _emailOptions = emailOptions.Value;
        _logger = logger;
    }

    public async Task SendEmail(SendEmailCommand command, CancellationToken cancellationToken)
    {
        var serialisedCommand = JsonSerializer.Serialize(command);

        LogSendingEmail(_emailOptions.QueueUrl);

        await _sqs.SendMessageAsync(
            new SendMessageRequest
            {
                QueueUrl = _emailOptions.QueueUrl,
                MessageBody = serialisedCommand,
            },
            cancellationToken
        );

        LogEmailSent(_emailOptions.QueueUrl);
    }

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Sending email message to SQS queue '{QueueUrl}'."
    )]
    private partial void LogSendingEmail(string queueUrl);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Email message sent to SQS queue '{QueueUrl}'."
    )]
    private partial void LogEmailSent(string queueUrl);
}
