using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace UKPS.Api.Tests.Utilities.Harnesses;

public sealed class MockLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentBag<LogEntry> _entries = [];

    public IReadOnlyCollection<LogEntry> Entries => _entries;

    public ILogger CreateLogger(string categoryName)
    {
        return new TestLogger(categoryName, _entries);
    }

    public void Dispose() { }

    private sealed class TestLogger(string categoryName, ConcurrentBag<LogEntry> entries) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        )
        {
            entries.Add(
                new LogEntry(
                    categoryName,
                    logLevel,
                    eventId,
                    formatter(state, exception),
                    exception
                )
            );
        }
    }
}
