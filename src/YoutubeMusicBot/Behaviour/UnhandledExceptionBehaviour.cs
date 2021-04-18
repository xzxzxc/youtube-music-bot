using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace YoutubeMusicBot.Behaviour
{
	public class UnhandledExceptionBehaviour<TRequest, TResponse> :
		IPipelineBehavior<TRequest, TResponse>
		where TRequest : IRequest
	{
		private readonly ILogger<TRequest> _logger;

		public UnhandledExceptionBehaviour(ILogger<TRequest> logger)
		{
			_logger = logger;
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
			catch (Exception exception)
			{
				var requestName = typeof(TResponse).Name;

				_logger.LogError(
					exception,
					"Request: Unhandled Exception for {Name} {@Request}",
					requestName,
					request);
				throw;
			}
		}
	}
}
