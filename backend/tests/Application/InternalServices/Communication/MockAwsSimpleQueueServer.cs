using Amazon.SQS;
using Amazon.SQS.Model;
using NSubstitute;

namespace UKPS.Api.Tests.Application.InternalServices.Communication;

internal sealed class MockAwsSimpleQueueServer
{
    public IAmazonSQS Mock { get; } = Substitute.For<IAmazonSQS>();
    public IEnumerable<Message> Messages => _sentMessages.Except(_deletedMessages);
    public IEnumerable<Message> DeletedMessages => _deletedMessages;

    /// <summary>The number of SendMessageBatch calls received.</summary>
    public int BatchRequestCount { get; private set; }

    /// <summary>When true, SendMessageBatch queues nothing and reports every entry as failed.</summary>
    public bool FailBatchEntries { get; set; }

    private readonly List<Message> _sentMessages = new List<Message>();
    private readonly List<Message> _deletedMessages = new List<Message>();

    public MockAwsSimpleQueueServer()
    {
        Mock.SendMessageAsync(Arg.Any<SendMessageRequest>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var request = callInfo.Arg<SendMessageRequest>();
                _sentMessages.Add(
                    new Message
                    {
                        Body = request.MessageBody,
                        ReceiptHandle = Guid.NewGuid().ToString(),
                    }
                );
                return Task.FromResult(
                    new SendMessageResponse() { MessageId = Guid.NewGuid().ToString() }
                );
            });

        Mock.SendMessageBatchAsync(Arg.Any<SendMessageBatchRequest>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var request = callInfo.Arg<SendMessageBatchRequest>();
                BatchRequestCount++;

                // SQS reports failed entries in the response and still returns success.
                if (FailBatchEntries)
                {
                    return Task.FromResult(
                        new SendMessageBatchResponse
                        {
                            Failed = request
                                .Entries.Select(e => new BatchResultErrorEntry
                                {
                                    Id = e.Id,
                                    Code = "InternalError",
                                })
                                .ToList(),
                        }
                    );
                }

                _sentMessages.AddRange(
                    request.Entries.Select(e => new Message
                    {
                        Body = e.MessageBody,
                        ReceiptHandle = Guid.NewGuid().ToString(),
                    })
                );
                return Task.FromResult(
                    new SendMessageBatchResponse
                    {
                        Successful = request
                            .Entries.Select(e => new SendMessageBatchResultEntry
                            {
                                Id = e.Id,
                                MessageId = Guid.NewGuid().ToString(),
                            })
                            .ToList(),
                    }
                );
            });

        Mock.ReceiveMessageAsync(Arg.Any<ReceiveMessageRequest>(), Arg.Any<CancellationToken>())
            .Returns(_ => new ReceiveMessageResponse { Messages = Messages.ToList() });

        Mock.DeleteMessageAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var receiptHandle = callInfo.ArgAt<string>(1);
                var messageToDelete = _sentMessages.FirstOrDefault(m =>
                    string.Equals(m.ReceiptHandle, receiptHandle, StringComparison.Ordinal)
                );
                if (messageToDelete != null)
                {
                    _deletedMessages.Add(messageToDelete);
                }
                return Task.FromResult(new DeleteMessageResponse());
            });
    }
}
