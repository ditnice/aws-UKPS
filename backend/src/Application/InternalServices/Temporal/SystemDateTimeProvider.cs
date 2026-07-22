namespace UKPS.Api.Application.InternalServices.Temporal;

internal class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime GetUtcNow() => DateTime.UtcNow;
}
