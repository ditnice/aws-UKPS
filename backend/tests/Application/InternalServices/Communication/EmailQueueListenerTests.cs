using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using UKPS.Api.Application.InternalServices.Communication;

namespace UKPS.Api.Tests.Application.InternalServices.Communication;

public class EmailQueueListenerTests
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

    [Fact]
    public async Task ExecuteAsync_ShouldPollQueue_WhenListenerIsEnabled()
    {
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var polled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        scopeFactory
            .CreateScope()
            .Returns(_ =>
            {
                polled.TrySetResult();
                throw new InvalidOperationException("Stop after the first poll.");
            });
        var options = Options.Create(
            new EmailOptions
            {
                FromAddress = "from@example.com",
                QueueUrl = "https://sqs.us-east-1.amazonaws.com/123456789012/my-queue",
                ListenerEnabled = true,
            }
        );
        using var listener = new EmailQueueListener(
            scopeFactory,
            options,
            NullLogger<EmailQueueListener>.Instance
        );

        await listener.StartAsync(TestContext.Current.CancellationToken);
        await polled.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await listener.StopAsync(TestContext.Current.CancellationToken);

        scopeFactory.Received().CreateScope();
    }
}
