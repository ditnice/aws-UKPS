using NSubstitute;
using UKPS.Api.Application.InternalServices.Communication;

namespace UKPS.Api.Tests.Utilities.MockInternalServices;

public class MockEmailService
{
    public IReadOnlyCollection<SendEmailCommand> SentCommands => _sent;
    public IReadOnlyCollection<IEmail> Sent => _sent.Select(x => x.Email).ToList();
    public IEmailService Mock { get; } = Substitute.For<IEmailService>();

    private readonly List<SendEmailCommand> _sent = new List<SendEmailCommand>();

    public MockEmailService()
    {
        Mock.SendEmail(Arg.Any<SendEmailCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var command = callInfo.Arg<SendEmailCommand>();
                _sent.Add(command);
                return Task.CompletedTask;
            });

        Mock.SendEmails(
                Arg.Any<IReadOnlyCollection<SendEmailCommand>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(callInfo =>
            {
                _sent.AddRange(callInfo.Arg<IReadOnlyCollection<SendEmailCommand>>());
                return Task.CompletedTask;
            });
    }

    internal void Clear()
    {
        _sent.Clear();
    }
}
