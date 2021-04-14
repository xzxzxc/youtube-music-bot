using System;
using Microsoft.Extensions.Logging;

namespace YoutubeMusicBot.Behaviour
{
	public abstract class BaseUnhandledExceptionBehaviour<T>
	{
		private readonly ILogger<T> _logger;

		protected BaseUnhandledExceptionBehaviour(ILogger<T> logger)
		{
			_logger = logger;
		}

		protected void LogException(Exception exception, T value)
		{
			var requestName = typeof(T).Name;

			_logger.LogError(
				exception,
				"Request: Unhandled Exception for {Name} {@Request}",
				requestName,
				value);
		}
	}
}
