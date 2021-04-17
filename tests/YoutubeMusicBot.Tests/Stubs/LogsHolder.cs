using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace YoutubeMusicBot.Tests.Stubs
{
	public static class LogsHolder
	{
		public static List<Execution> Executions { get; } = new();

		public record Execution(
			LogLevel LogLevel,
			string? Message,
			Exception? Exception)
		{
		}
	}
}
