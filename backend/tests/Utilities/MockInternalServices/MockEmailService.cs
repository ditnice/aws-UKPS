using NSubstitute;
using UKPS.Api.Application.InternalServices.Communication;

namespace UKPS.Api.Tests.Utilities.MockInternalServices;

public class MockEmailService
{
    public IReadOnlyCollection<IEmail> Sent => _sent;
    public IEmailService Mock { get; } = Substitute.For<IEmailService>();

    private readonly List<IEmail> _sent = new List<IEmail>();

    public MockEmailService()
    {
        Mock.SendEmail(Arg.Any<SendEmailCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var command = callInfo.Arg<SendEmailCommand>();
                _sent.Add(command.Email);
                return Task.CompletedTask;
            });
    }
}
