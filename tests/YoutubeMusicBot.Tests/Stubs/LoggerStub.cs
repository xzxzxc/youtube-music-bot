using System;
using Microsoft.Extensions.Logging;
using Moq;

namespace YoutubeMusicBot.Tests.Stubs
{
	public class LoggerStub<T> : ILogger<T>
	{
		public void Log<TState>(
			LogLevel logLevel,
			EventId eventId,
			TState state,
			Exception? exception,
			Func<TState, Exception?, string> formatter)
		{
			LogsHolder.Executions.Add(
				new LogsHolder.Execution(
					logLevel,
					formatter(state, exception),
					exception));
		}

		public bool IsEnabled(LogLevel logLevel) =>
			true;

		public IDisposable BeginScope<TState>(TState state) =>
			Mock.Of<IDisposable>();
	}
}
