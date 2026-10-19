using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using UKPS.Api.Application.InternalServices.Communication;

namespace UKPS.Api.Tests.Application.InternalServices.Communication;

public sealed class EmailQueueListenerTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldNotPollQueue_WhenListenerIsDisabled()
    {
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var options = Options.Create(
            new EmailOptions
            {
                FromAddress = "from@example.com",
                QueueUrl = "https://sqs.us-east-1.amazonaws.com/123456789012/my-queue",
                ListenerEnabled = false,
            }
        );
        using var listener = new EmailQueueListener(
            scopeFactory,
            options,
            NullLogger<EmailQueueListener>.Instance
        );

        await listener.StartAsync(TestContext.Current.CancellationToken);
        await listener.ExecuteTask!;

        scopeFactory.DidNotReceive().CreateScope();
    }
}
