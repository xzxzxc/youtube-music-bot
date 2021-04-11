using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace YoutubeMusicBot.Behaviour
{
	public class UnhandledExceptionBehaviour<TNotification> :
		INotificationBehavior<TNotification>
		where TNotification : INotification
	{
		private readonly ILogger<TNotification> _logger;

		public UnhandledExceptionBehaviour(ILogger<TNotification> logger)
		{
			_logger = logger;
		}

		public async Task Handle(
			TNotification notification,
			CancellationToken cancellationToken,
			Func<Task> next)
		{
			try
			{
				await next();
			}
			catch (Exception ex)
			{
				var requestName = typeof(TNotification).Name;

				_logger.LogError(
					ex,
					"Request: Unhandled Exception for Request {Name} {@Request}",
					requestName,
					notification);

				throw;
			}
		}
	}

	public interface INotificationBehavior<TNotification>
		where TNotification : INotification
	{
		Task Handle(
			TNotification notification,
			CancellationToken cancellationToken,
			Func<Task> next);
	}
}
