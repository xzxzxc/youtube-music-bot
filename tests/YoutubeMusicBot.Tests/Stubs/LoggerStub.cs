using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;

namespace YoutubeMusicBot.Tests.Stubs
{
	public class LoggerStub<T> : ILogger<T>
	{
		private static readonly List<Execution> _executions = new();
		public static IReadOnlyList<Execution> Executions => _executions;

		public void Log<TState>(
			LogLevel logLevel,
			EventId eventId,
			TState state,
			Exception exception,
			Func<TState, Exception, string> formatter)
		{
			_executions.Add(new Execution(
				logLevel,
				formatter(state, exception),
				exception));
		}

		public bool IsEnabled(LogLevel logLevel) =>
			true;

		public IDisposable BeginScope<TState>(TState state) =>
			Mock.Of<IDisposable>();

		public record Execution(
			LogLevel LogLevel,
			string Message,
			Exception Exception)
		{
		}
	}
}
