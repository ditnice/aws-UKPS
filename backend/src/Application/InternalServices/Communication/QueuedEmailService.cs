using System.Globalization;
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

    public async Task SendEmails(
        IReadOnlyCollection<SendEmailCommand> commands,
        CancellationToken cancellationToken
    )
    {
        int failedCount = 0;

        foreach (SendEmailCommand[] batch in commands.Chunk(10))
        {
            var request = new SendMessageBatchRequest
            {
                QueueUrl = _emailOptions.QueueUrl,
                Entries = batch
                    .Select(
                        (command, index) =>
                            new SendMessageBatchRequestEntry
                            {
                                Id = index.ToString(CultureInfo.InvariantCulture),
                                MessageBody = JsonSerializer.Serialize(command),
                            }
                    )
                    .ToList(),
            };

            SendMessageBatchResponse response = await _sqs.SendMessageBatchAsync(
                request,
                cancellationToken
            );

            if (response.Failed is { Count: > 0 })
            {
                failedCount += response.Failed.Count;
                LogEmailBatchFailed(
                    _emailOptions.QueueUrl,
                    response.Failed.Count,
                    response.Failed[0].Code
                );
            }
        }

        if (failedCount > 0)
        {
            throw new InvalidOperationException(
                $"{failedCount} email message(s) could not be queued."
            );
        }
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

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "{FailedCount} email message(s) could not be sent to SQS queue '{QueueUrl}'. First error code: {ErrorCode}."
    )]
    private partial void LogEmailBatchFailed(string queueUrl, int failedCount, string errorCode);
}
