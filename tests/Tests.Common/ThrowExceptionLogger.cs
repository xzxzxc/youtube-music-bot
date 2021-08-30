using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.Logging;

namespace YoutubeMusicBot.IntegrationTests.Common
{
    public static class ThrowExceptionLogger
    {
        public static ConcurrentBag<ExceptionDispatchInfo> Errors { get; } = new();

        public static void ThrowIfNotEmpty()
        {
            if (Errors.IsEmpty)
                return;

            if (Errors.Count == 1)
                Errors.First().Throw();

            throw new AggregateException(Errors.Select(e => e.SourceException));
        }
    }

    public class ThrowExceptionLogger<T> : ILogger<T>,
        IDisposable
    {
        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (logLevel >= LogLevel.Error)
            {
                exception ??= new Exception(formatter(state, exception));
                var info = ExceptionDispatchInfo.Capture(exception);
                ThrowExceptionLogger.Errors.Add(info);
                info.Throw();
            }
        }

        public bool IsEnabled(LogLevel logLevel) =>
            true;

        public IDisposable BeginScope<TState>(TState state) =>
            this;

        public void Dispose()
        {
        }
    }
}
