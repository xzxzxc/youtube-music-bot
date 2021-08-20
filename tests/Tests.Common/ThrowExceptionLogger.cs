using System;
using Microsoft.Extensions.Logging;

namespace YoutubeMusicBot.Tests.Common
{
    public class ThrowExceptionLogger<T> : ILogger<T>,
        IDisposable
    {
        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception, string> formatter)
        {
            if (logLevel >= LogLevel.Error)
                throw exception
                    ?? new Exception(formatter(state, new Exception()));
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
