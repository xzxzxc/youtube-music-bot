using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Application.Mediator.Implementation
{
    public class ExceptionLogDecorator : IMediator
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public ExceptionLogDecorator(
            IMediator mediator,
            ILogger<ExceptionLogDecorator> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async ValueTask Send<TRequest>(
            TRequest request,
            CancellationToken cancellationToken = default)
            where TRequest : IRequest
        {
            try
            {
                await _mediator.Send(request, cancellationToken);
            }
            catch (Exception ex) when (ex is not (OperationCanceledException or DecoratedException))
            {
                LogException(ex, request);
                throw new DecoratedException(ex);
            }
        }

        public async ValueTask<TResult> Send<TRequest, TResult>(
            TRequest request,
            CancellationToken cancellationToken = default)
            where TRequest : IRequest<TResult>
        {
            try
            {
                return await _mediator.Send<TRequest, TResult>(request, cancellationToken);
            }
            catch (Exception ex) when (ex is not (OperationCanceledException or DecoratedException))
            {
                LogException(ex, request);
                throw new DecoratedException(ex);
            }
        }

        public async ValueTask Emit<TAggregate>(
            EventBase<TAggregate> @event,
            CancellationToken cancellationToken = default)
            where TAggregate : AggregateBase<TAggregate>
        {
            try
            {
                await _mediator.Emit(@event, cancellationToken);
            }
            catch (Exception ex) when (ex is not (OperationCanceledException or DecoratedException))
            {
                LogException(ex, @event);
                throw new DecoratedException(ex);
            }
        }

        public void Cancel(string eventCancellationId)
        {
            try
            {
                _mediator.Cancel(eventCancellationId);
            }
            catch (Exception ex) when (ex is not (OperationCanceledException or DecoratedException))
            {
                LogException(ex, eventCancellationId);
                throw new DecoratedException(ex);
            }
        }

        private void LogException<T>(Exception exception, T value)
        {
            var requestName = typeof(T).Name;
            _logger.LogError(
                exception,
                "Request: Unhandled Exception for {Name} {@Request}",
                requestName,
                value);
        }

        /// <summary>
        /// Used to prevent from duplicates of exception in logs
        /// </summary>
        private class DecoratedException : Exception
        {
            public DecoratedException(Exception inner)
                : base(inner.Message, inner)
            {
            }
        }
    }
}
