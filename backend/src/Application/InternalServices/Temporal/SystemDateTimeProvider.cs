namespace UKPS.Api.Application.InternalServices.Temporal;

internal class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset GetOffsetUtcNow() => DateTimeOffset.UtcNow;

    public DateTime GetUtcNow() => DateTime.UtcNow;
}
