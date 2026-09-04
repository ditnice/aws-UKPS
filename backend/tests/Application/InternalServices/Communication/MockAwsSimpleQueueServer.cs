using Amazon.SQS;
using Amazon.SQS.Model;
using NSubstitute;

namespace UKPS.Api.Tests.Application.InternalServices.Communication;

internal sealed class MockAwsSimpleQueueServer
{
    public IAmazonSQS Mock { get; } = Substitute.For<IAmazonSQS>();
    public IEnumerable<Message> Messages => _sentMessages.Except(_deletedMessages);
    public IEnumerable<Message> DeletedMessages => _deletedMessages;

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
