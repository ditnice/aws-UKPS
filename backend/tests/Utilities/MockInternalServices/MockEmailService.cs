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
        Mock.WhenForAnyArgs(x => x.SendEmail(default!, default!, default!))
            .Do((callInfo) => _sent.Add(callInfo.Arg<IEmail>()));
    }
}
