using Microsoft.Extensions.Logging;

namespace UKPS.Api.Tests.Utilities.Harnesses;

public sealed record LogEntry(
    string CategoryName,
    LogLevel LogLevel,
    EventId EventId,
    string Message,
    Exception? Exception
);
