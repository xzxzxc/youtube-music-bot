using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Application.Mediator.Implementation
{
    public class MediatorService : IMediator
    {
        private readonly ILifetimeScope _scope;
        private readonly ConcurrentDictionary<string, Action> _cancellationActionsCache = new();

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
            using var cancellationHolder = new CancellationHolder<TAggregate>(
                @event,
                cancellationToken,
                _cancellationActionsCache);
            var handlerType = typeof(IEventHandler<,>).MakeGenericType(
                @event.GetType(),
                typeof(TAggregate));
            await using var scope = _scope.BeginLifetimeScope();
            dynamic handler = scope.Resolve(handlerType);
            await handler.Handle((dynamic)@event, cancellationHolder.CancellationToken);
        }

        public void Cancel<TAggregate>(long aggregateId)
            where TAggregate : AggregateBase<TAggregate>
        {
            var cancellationAction = CancellationHolder<TAggregate>.GetCancelAction(
                aggregateId,
                _cancellationActionsCache);
            cancellationAction();
        }

        private class CancellationHolder<TAggregate> : IDisposable
            where TAggregate : AggregateBase<TAggregate>
        {
            private readonly string _cacheKey;
            private readonly ConcurrentDictionary<string, Action> _cancellationActions;
            private readonly CancellationTokenSource _aggregateSource;

            public CancellationHolder(
                EventBase<TAggregate> @event,
                CancellationToken initialToken,
                ConcurrentDictionary<string, Action> cancellationActions)
            {
                _cancellationActions = cancellationActions;
                _cacheKey = CreateCacheKey(@event.AggregateId);
                _aggregateSource = CancellationTokenSource.CreateLinkedTokenSource(
                    initialToken);
                _cancellationActions.GetOrAdd(_cacheKey, Cancel);
            }

            private void Cancel() =>
                _aggregateSource.Cancel();

            public CancellationToken CancellationToken => _aggregateSource.Token;

            public void Dispose() =>
                _cancellationActions.Remove(_cacheKey, out _);

            public static Action GetCancelAction(
                long aggregateId,
                ConcurrentDictionary<string, Action> cancellationActionsCache)
            {
                var cacheKey = CreateCacheKey(aggregateId);
                return cancellationActionsCache[cacheKey];
            }

            private static string CreateCacheKey(long aggregateId) =>
                $"{typeof(TAggregate).Name}{aggregateId}";
        }
    }
}
