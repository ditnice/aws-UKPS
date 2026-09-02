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
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Tests.Utilities.Harnesses;

namespace UKPS.Api.Tests.Application.InternalServices.Communication;

public class EmailServiceTests
{
    private readonly IEmailService _sut;
    private readonly IAmazonSimpleEmailServiceV2 _mockEmailService;

    public MockLoggerProvider Logs { get; } = new();

    private readonly string _fromAddress = "examplefromaddress@email.com";

    private readonly SendEmailCommand _validEmailCommand = new SendEmailCommandFaker().Generate();

    public EmailServiceTests()
    {
        _mockEmailService = Substitute.For<IAmazonSimpleEmailServiceV2>();
        _mockEmailService
            .SendEmailAsync(Arg.Any<SendEmailRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SendEmailResponse() { MessageId = "123" });
        var serviceCollection = new ServiceCollection()
            .AddSingleton(_ => Options.Create(new EmailOptions() { FromAddress = _fromAddress }))
            .AddEmailServices()
            .AddTransient(_ => _mockEmailService)
            .AddLogging(x => x.AddProvider(Logs));

        _sut = serviceCollection.BuildServiceProvider().GetRequiredService<IEmailService>();
    }

    [Fact]
    public async Task SendEmail_ShouldSendAnEmailViaSes()
    {
        var htmlContent = _validEmailCommand.Email.GetHtmlContent();
        await _sut.SendEmail(_validEmailCommand, TestContext.Current.CancellationToken);

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
    public async Task SendEmail_OnExceptionFromService_ShouldLogAndPassOnException()
    {
        var exception = new InvalidOperationException("Test Exception");
        _mockEmailService
            .SendEmailAsync(Arg.Any<SendEmailRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        InvalidOperationException foundException =
            await Should.ThrowAsync<InvalidOperationException>(async () =>
            {
                await _sut.SendEmail(_validEmailCommand, TestContext.Current.CancellationToken);
            });
        foundException.Message.ShouldBe(exception.Message);

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
            RuleFor(x => x.Email, _ => new TestEmail());
        }
    }

    private sealed class TestEmail : IEmail
    {
        public string Subject => "Test";

        public string GetHtmlContent()
        {
            return "<p>Test</p>";
        }
    }
}
