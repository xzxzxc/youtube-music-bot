using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using YoutubeMusicBot.Application.Extensions;
using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Application.Mediator.Implementation
{
    public class MediatorService : IMediator
    {
        private readonly ILifetimeScope _scope;
        private readonly ConcurrentDictionary<string, Action> _cancellationActions = new();

        public MediatorService(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public async ValueTask Send<TRequest>(
            TRequest request,
            CancellationToken cancellationToken = default)
            where TRequest : IRequest
        {
            await using var scope = _scope.BeginLifetimeScope();
            var handler = scope.Resolve<IRequestHandler<TRequest>>();
            await handler.Handle(request, cancellationToken);
        }

        public async ValueTask<TResult> Send<TRequest, TResult>(
            TRequest request,
            CancellationToken cancellationToken = default)
            where TRequest : IRequest<TResult>
        {
            await using var scope = _scope.BeginLifetimeScope();
            var handler = scope.Resolve<IRequestHandler<TRequest, TResult>>();
            return await handler.Handle(request, cancellationToken);
        }

        public async ValueTask Emit<TAggregate>(
            EventBase<TAggregate> @event,
            CancellationToken cancellationToken = default)
            where TAggregate : AggregateBase<TAggregate>
        {
            using var cancellationHolder = new CancellationHolder(
                @event.GetCancellationId(),
                cancellationToken,
                _cancellationActions);
            var handlerType = typeof(IEventHandler<,>).MakeGenericType(
                @event.GetType(),
                typeof(TAggregate));
            await using var scope = _scope.BeginLifetimeScope();
            dynamic handler = scope.Resolve(handlerType);
            await handler.Handle((dynamic)@event, cancellationHolder.CancellationToken);
        }

        public void Cancel(string eventCancellationId)
        {
            var cancellationAction = _cancellationActions[eventCancellationId];
            cancellationAction();
        }

        private class CancellationHolder : IDisposable
        {
            private readonly string _eventCancellationId;
            private readonly ConcurrentDictionary<string, Action> _cancellationActions;
            private readonly CancellationTokenSource _aggregateSource;

            public CancellationHolder(
                string eventCancellationId,
                CancellationToken initialToken,
                ConcurrentDictionary<string, Action> cancellationActions)
            {
                _eventCancellationId = eventCancellationId;
                _cancellationActions = cancellationActions;
                _aggregateSource = CancellationTokenSource.CreateLinkedTokenSource(
                    initialToken);
                _cancellationActions.AddOrUpdate(
                    eventCancellationId,
                    Cancel,
                    (key, _) => throw new InvalidOperationException(
                        $"There is value with the same key: {key}."));
            }

            private void Cancel() =>
                _aggregateSource.Cancel();

            public CancellationToken CancellationToken => _aggregateSource.Token;

            public void Dispose() =>
                _cancellationActions.Remove(_eventCancellationId, out _);
        }
    }
}
