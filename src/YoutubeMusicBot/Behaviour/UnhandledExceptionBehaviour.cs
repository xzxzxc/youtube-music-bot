using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace YoutubeMusicBot.Behaviour
{
	public class UnhandledExceptionBehaviour<TNotification> :
		BaseUnhandledExceptionBehaviour<TNotification>,
		INotificationBehavior<TNotification>
		where TNotification : INotification
	{
		public UnhandledExceptionBehaviour(ILogger<TNotification> logger)
			: base(logger)
		{
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
				LogException(ex, notification);
				throw;
			}
		}
	}

	public class UnhandledExceptionBehaviour<TRequest, TResponse> :
		BaseUnhandledExceptionBehaviour<TRequest>,
		IPipelineBehavior<TRequest, TResponse>
		where TRequest : IRequest
	{
		public UnhandledExceptionBehaviour(ILogger<TRequest> logger)
			: base(logger)
		{
		}

		public async Task<TResponse> Handle(
			TRequest request,
			CancellationToken cancellationToken,
			RequestHandlerDelegate<TResponse> next)
		{
			try
			{
				return await next();
			}
			catch (Exception ex)
			{
				LogException(ex, request);
				throw;
			}
		}
	}
}
