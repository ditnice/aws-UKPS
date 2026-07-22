using UKPS.Api.Application.InternalServices.Temporal;

namespace UKPS.Api.Tests.Utilities.MockInternalServices;

public class FakeDateTimeProvider : IDateTimeProvider
{
    private readonly DateTime _dateTime;

    public FakeDateTimeProvider(DateTime dateTime)
    {
        _dateTime = dateTime;
    }

    public DateTime GetUtcNow()
    {
        return _dateTime;
    }
}
