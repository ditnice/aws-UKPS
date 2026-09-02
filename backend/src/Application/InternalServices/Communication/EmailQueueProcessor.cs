using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;

namespace UKPS.Api.Application.InternalServices.Communication;

internal sealed partial class EmailQueueProcessor
{
    private readonly IAmazonSQS _sqs;
    private readonly EmailQueueMessageHandler _emailQueueHandler;
    private readonly ILogger<EmailQueueProcessor> _logger;
    private readonly EmailOptions _emailOptions;

    public EmailQueueProcessor(
        IAmazonSQS sqs,
        EmailQueueMessageHandler emailQueueHandler,
        IOptions<EmailOptions> emailOptions,
        ILogger<EmailQueueProcessor> logger
    )
    {
        _sqs = sqs;
        _emailQueueHandler = emailQueueHandler;
        _emailOptions = emailOptions.Value;
        _logger = logger;
    }

    public async Task ProcessEmailQueue(CancellationToken cancellationToken)
    {
        var receiveRequest = new ReceiveMessageRequest
        {
            QueueUrl = _emailOptions.QueueUrl,
            MaxNumberOfMessages = 10,
            WaitTimeSeconds = 20,
        };

        var receiveResponse = await _sqs.ReceiveMessageAsync(receiveRequest, cancellationToken);

        foreach (var message in receiveResponse.Messages ?? [])
        {
#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                await _emailQueueHandler.HandleEmailQueueMessage(message.Body, cancellationToken);
                await _sqs.DeleteMessageAsync(
                    _emailOptions.QueueUrl,
                    message.ReceiptHandle,
                    cancellationToken
                );
            }
            catch (Exception ex)
            {
                LogEmailHandlingFailure(ex, message.ReceiptHandle);
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "An error occur whilst handling email message. [ReceiptHandle: {ReceiptHandle}]"
    )]
    private partial void LogEmailHandlingFailure(Exception ex, string receiptHandle);
}
