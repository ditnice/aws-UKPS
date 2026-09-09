using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Bogus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using UKPS.Api.Application.InternalServices.Communication;
using UKPS.Api.Application.Users;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Tests.Utilities.Harnesses;

namespace UKPS.Api.Tests.Application.InternalServices.Communication;

public class EmailServiceTests
{
    private readonly EmailQueueProcessor _processor;
    private readonly IEmailService _sut;
    private readonly MockAwsSimpleQueueServer _mockAwsSqs;
    private readonly IAmazonSimpleEmailServiceV2 _mockEmailService;

    public MockLoggerProvider Logs { get; } = new();

    private readonly string _fromAddress = "examplefromaddress@email.com";

    private readonly SendEmailCommand _validEmailCommand = new SendEmailCommandFaker().Generate();

    public EmailServiceTests()
    {
        _mockAwsSqs = new MockAwsSimpleQueueServer();
        _mockEmailService = Substitute.For<IAmazonSimpleEmailServiceV2>();
        _mockEmailService
            .SendEmailAsync(Arg.Any<SendEmailRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SendEmailResponse() { MessageId = "123" });
        var serviceCollection = new ServiceCollection()
            .AddSingleton(_ =>
                Options.Create(
                    new EmailOptions()
                    {
                        FromAddress = _fromAddress,
                        QueueUrl = "https://sqs.us-east-1.amazonaws.com/123456789012/my-queue",
                    }
                )
            )
            .AddEmailServices()
            .AddTransient(_ => _mockEmailService)
            .AddSingleton(_ => _mockAwsSqs.Mock)
            .AddLogging(x => x.AddProvider(Logs));

        var serviceProvider = serviceCollection.BuildServiceProvider();
        _processor = serviceProvider.GetRequiredService<EmailQueueProcessor>();
        _sut = serviceProvider.GetRequiredService<IEmailService>();
    }

    [Fact]
    public async Task SendEmail_ShouldSendAnEmailViaSes()
    {
        var htmlContent = _validEmailCommand.Email.GetHtmlContent();
        await _sut.SendEmail(_validEmailCommand, TestContext.Current.CancellationToken);
        await _processor.ProcessEmailQueue(TestContext.Current.CancellationToken);

        await _mockEmailService
            .Received(1)
            .SendEmailAsync(
                Arg.Is<SendEmailRequest>(x =>
                    x.FromEmailAddress == _fromAddress
                    && x.Destination.ToAddresses.Single() == _validEmailCommand.RecipientAddress
                    && x.Content.Simple.Subject.Data == _validEmailCommand.Email.Subject
                    && x.Content.Simple.Body.Html.Data.Contains(htmlContent)
                ),
                Arg.Any<CancellationToken>()
            );

        AssertUserEmailNotLogged(_validEmailCommand.RecipientAddress);
    }

    [Fact]
    public async Task SendEmail_OnSuccess_DeleteMessage()
    {
        await _sut.SendEmail(_validEmailCommand, TestContext.Current.CancellationToken);
        await _processor.ProcessEmailQueue(TestContext.Current.CancellationToken);

        _mockAwsSqs.Messages.Count().ShouldBe(0);
        _mockAwsSqs.DeletedMessages.Count().ShouldBe(1);
    }

    [Fact]
    public async Task SendEmail_OnExceptionFromService_ShouldNotDeleteMessage()
    {
        var exception = new InvalidOperationException("Test Exception");
        _mockEmailService
            .SendEmailAsync(Arg.Any<SendEmailRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        await _sut.SendEmail(_validEmailCommand, TestContext.Current.CancellationToken);
        await _processor.ProcessEmailQueue(TestContext.Current.CancellationToken);
        _mockAwsSqs.Messages.Count().ShouldBe(1);

        AssertUserEmailNotLogged(_validEmailCommand.RecipientAddress);
    }

    private void AssertUserEmailNotLogged(string userEmail)
    {
        foreach (var logMessage in Logs.Entries)
        {
            logMessage.Message.ShouldNotContain(userEmail, caseSensitivity: Case.Insensitive);
        }
    }

    private sealed class SendEmailCommandFaker : Faker<SendEmailCommand>
    {
        public SendEmailCommandFaker()
        {
            StrictMode(true);
            RuleFor(
                x => x.CognitoUsername,
                f => new CognitoUsername() { Value = f.Random.Guid().ToString() }
            );
            RuleFor(x => x.RecipientAddress, f => f.Internet.Email());
            RuleFor(
                x => x.Email,
                f => new UserSignUpRequestEmail() { Link = new Uri(f.Internet.Url()) }
            );
        }
    }
}
