using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace YoutubeMusicBot.Tests.Common
{
    public static class ThrowExceptionLogger
    {
        public static ConcurrentBag<Exception> Errors { get; } = new();
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
                ThrowExceptionLogger.Errors.Add(exception);
                throw exception;
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
