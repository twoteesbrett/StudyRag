using StudyRag.Core.Logging;
using Microsoft.Extensions.Logging;

namespace StudyRag.Helpers;

public sealed class DebugConsoleLoggerProvider : ILoggerProvider
{
    private static readonly object OutputLock = new();

    public ILogger CreateLogger(string categoryName) => new ConsoleLogger();
    public void Dispose() { }

    private sealed class ConsoleLogger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var color = logLevel switch
            {
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error or LogLevel.Critical => ConsoleColor.Red,
                _ when eventId == LogEvents.Action => ConsoleColor.Red,
                _ when eventId == LogEvents.Response => ConsoleColor.DarkYellow,
                _ => ConsoleColor.Green
            };

            lock (OutputLock)
            {
                foreach (var line in formatter(state, exception).Replace("\r\n", "\n").Split('\n'))
                {
                    var isHeader = line.StartsWith("=== ") && line.EndsWith(" ===");
                    DebugConsole.WriteLine(line, isHeader ? ConsoleColor.Yellow : color);
                }

                if (exception is not null)
                    DebugConsole.WriteLine(exception.ToString(), ConsoleColor.Red);
            }
        }
    }
}
